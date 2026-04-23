using System.Globalization;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImovelStand.Infrastructure.Precificacao;

/// <summary>
/// Motor de precificação dinâmica: analisa velocidade de absorção interna
/// + benchmark de mercado (FIPE-ZAP ou equivalente) e sugere ajustes.
///
/// Algoritmo MVP:
/// - Se tipologia vende >= 150% da média do mercado E preço <= mercado:
///   sugerir aumento 3-8%
/// - Se unidade encalhada (> 120 dias no status Disponivel) E preço > mercado:
///   sugerir desconto 2-6%
/// - Se preço < mercado - 1 desvio padrão: sugerir aumento para mediana
///
/// Respeita política: max variação ±10% por sugestão, min 20 amostras de
/// mercado. Confiança baixa (<50) não gera sugestão.
/// </summary>
public class PrecificacaoService
{
    private const int DIAS_ENCALHE = 120;
    private const int MIN_AMOSTRAS_MERCADO = 20;
    private const decimal MAX_VARIACAO = 0.10m;
    private const int MIN_CONFIANCA = 50;

    private readonly ApplicationDbContext _context;
    private readonly ILogger<PrecificacaoService> _logger;

    public PrecificacaoService(ApplicationDbContext context, ILogger<PrecificacaoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Calcula sugestão para um apartamento específico. Retorna null se
    /// não há dados suficientes ou se nenhuma condição é acionada.
    /// </summary>
    public async Task<SugestaoPreco?> CalcularSugestaoAsync(int apartamentoId, CancellationToken ct = default)
    {
        var apto = await _context.Apartamentos
            .Include(a => a.Torre!)
                .ThenInclude(t => t.Empreendimento!)
            .Include(a => a.Tipologia)
            .FirstOrDefaultAsync(a => a.Id == apartamentoId, ct);

        if (apto is null) return null;
        if (apto.Status != StatusApartamento.Disponivel) return null;
        if (apto.Tipologia is null || apto.Torre is null) return null;

        var empreendimento = apto.Torre.Empreendimento;
        var endereco = empreendimento?.Endereco;
        if (endereco is null)
        {
            _logger.LogDebug("Apartamento {Id} sem endereco do empreendimento — pulando", apartamentoId);
            return null;
        }

        // 1. Mercado
        var mercado = await _context.Set<PrecoMercado>().AsNoTracking()
            .Where(m => m.Cidade == endereco.Cidade
                     && m.Uf == endereco.Uf
                     && m.Quartos == apto.Tipologia.Quartos
                     && m.QtdAmostras >= MIN_AMOSTRAS_MERCADO)
            .OrderByDescending(m => m.DataReferencia)
            .FirstOrDefaultAsync(ct);

        // 2. Velocidade de venda interna para tipologia
        var dozeSemanasAtras = DateTime.UtcNow.AddDays(-84);
        var vendasTipologia = await _context.Vendas.AsNoTracking()
            .Where(v => v.DataFechamento >= dozeSemanasAtras
                     && _context.Apartamentos.Any(a => a.Id == v.ApartamentoId
                                                    && a.TipologiaId == apto.TipologiaId))
            .CountAsync(ct);
        var velocidadeTipologia = vendasTipologia / 12m;

        // 3. Tempo desde ultima mudança de status (aproximação de "tempo em vitrine")
        // Sem HistoricoStatusApartamento explícito, usamos DataCadastro como
        // proxy. Refinar depois.
        var diasEmVitrine = (int)(DateTime.UtcNow - apto.DataCadastro).TotalDays;

        // 4. Analise
        var precoAtual = apto.PrecoAtual;
        var precoM2Atual = apto.Tipologia.AreaPrivativa > 0
            ? precoAtual / apto.Tipologia.AreaPrivativa
            : 0m;

        string? motivo = null;
        string? justificativa = null;
        decimal precoSugerido = precoAtual;
        int confianca = 0;
        var velocidadeMercado = 1.0m; // default se nao tem benchmark

        if (mercado is not null && precoM2Atual > 0)
        {
            velocidadeMercado = 1.5m; // TODO: calcular de dataset mercado real
            var precoM2Medio = mercado.PrecoMedioM2;
            var desvio = mercado.DesvioPadraoM2;

            if (precoM2Atual < precoM2Medio - desvio
                && diasEmVitrine < DIAS_ENCALHE
                && velocidadeTipologia > 0.5m)
            {
                // Abaixo da media + vendendo bem → aumentar
                var variacao = Math.Min(MAX_VARIACAO, (precoM2Medio / precoM2Atual) - 1m);
                precoSugerido = Math.Round(precoAtual * (1 + variacao), 2);
                motivo = "abaixo-do-mercado";
                justificativa = $"Preço por m² (R$ {precoM2Atual.ToString("N2", new CultureInfo("pt-BR"))}) " +
                                $"está {FormatPct(1 - precoM2Atual / precoM2Medio)} abaixo da média da região " +
                                $"(R$ {precoM2Medio.ToString("N2", new CultureInfo("pt-BR"))}). " +
                                $"Considere aumentar {FormatPct(variacao)}.";
                confianca = 75;
            }
            else if (precoM2Atual > precoM2Medio + desvio
                     && diasEmVitrine >= DIAS_ENCALHE
                     && velocidadeTipologia < 0.2m)
            {
                // Acima da media + encalhado → desconto
                var variacao = Math.Min(0.06m, precoM2Atual / precoM2Medio - 1m);
                precoSugerido = Math.Round(precoAtual * (1 - variacao), 2);
                motivo = "encalhado-acima-mercado";
                justificativa = $"Unidade disponível há {diasEmVitrine} dias, acima do mercado. " +
                                $"Considere desconto de {FormatPct(variacao)} para acelerar venda.";
                confianca = 70;
            }
        }

        // Sem dados de mercado: baseia só em velocidade e tempo
        if (motivo is null)
        {
            if (diasEmVitrine >= DIAS_ENCALHE && velocidadeTipologia < 0.2m)
            {
                precoSugerido = Math.Round(precoAtual * 0.97m, 2);
                motivo = "encalhado";
                justificativa = $"Unidade disponível há {diasEmVitrine} dias com baixa velocidade de venda ({velocidadeTipologia:N1}/semana). " +
                                $"Considere desconto de 3%.";
                confianca = 55;
            }
            else if (velocidadeTipologia > 1.5m && diasEmVitrine < 30)
            {
                precoSugerido = Math.Round(precoAtual * 1.03m, 2);
                motivo = "vende-rapido";
                justificativa = $"Tipologia vende {velocidadeTipologia:N1}/semana — alta demanda. " +
                                $"Considere aumentar 3% no estoque remanescente.";
                confianca = 60;
            }
        }

        if (motivo is null || confianca < MIN_CONFIANCA || precoSugerido == precoAtual)
        {
            return null;
        }

        var sugestao = new SugestaoPreco
        {
            TenantId = apto.TenantId,
            ApartamentoId = apartamentoId,
            PrecoAtual = precoAtual,
            PrecoSugerido = precoSugerido,
            VariacaoPct = Math.Round((precoSugerido / precoAtual - 1m) * 100m, 2),
            Motivo = motivo,
            Justificativa = justificativa ?? string.Empty,
            VelocidadeVendaSemanal = velocidadeTipologia,
            VelocidadeMercado = velocidadeMercado,
            Confianca = confianca,
            Status = "pendente",
            CreatedAt = DateTime.UtcNow
        };

        return sugestao;
    }

    /// <summary>
    /// Roda o motor para todos os apartamentos Disponiveis de um tenant.
    /// Usado em job semanal.
    /// </summary>
    public async Task<int> GerarSugestoesParaTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var aptIds = await _context.Apartamentos.IgnoreQueryFilters()
            .Where(a => a.TenantId == tenantId && a.Status == StatusApartamento.Disponivel)
            .Select(a => a.Id)
            .ToListAsync(ct);

        var gerados = 0;
        foreach (var id in aptIds)
        {
            // Expira sugestões pendentes antigas (> 30 dias) do mesmo apto
            var antigas = await _context.Set<SugestaoPreco>().IgnoreQueryFilters()
                .Where(s => s.ApartamentoId == id
                         && s.TenantId == tenantId
                         && s.Status == "pendente"
                         && s.CreatedAt < DateTime.UtcNow.AddDays(-30))
                .ToListAsync(ct);
            foreach (var a in antigas) a.Status = "expirada";

            // Evita duplicata: só gera se não há sugestão pendente fresca
            var temFresca = await _context.Set<SugestaoPreco>().IgnoreQueryFilters()
                .AnyAsync(s => s.ApartamentoId == id
                            && s.TenantId == tenantId
                            && s.Status == "pendente"
                            && s.CreatedAt >= DateTime.UtcNow.AddDays(-7), ct);
            if (temFresca) continue;

            var sug = await CalcularSugestaoAsync(id, ct);
            if (sug is not null)
            {
                _context.Set<SugestaoPreco>().Add(sug);
                gerados++;
            }
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("PrecificacaoService: gerou {N} sugestoes para tenant {T}", gerados, tenantId);
        return gerados;
    }

    private static string FormatPct(decimal value) => $"{Math.Abs(value) * 100:N1}%";
}
