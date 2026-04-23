using System.Text.Json;
using ImovelStand.Application.Abstractions;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImovelStand.Infrastructure.OpenFinance;

/// <summary>
/// Orquestra o fluxo de análise de crédito via Open Finance:
/// 1. Corretor clica "Solicitar análise" → cria SolicitacaoAnaliseCredito
/// 2. Sistema gera link de autorização (ConnectLink via IOpenFinanceProvider)
/// 3. Cliente recebe link (WhatsApp/email), autoriza no banco dele
/// 4. Callback marca EmProcessamento → sistema puxa extratos
/// 5. AnaliseCreditoService calcula renda, volatilidade, dívidas, capacidade
/// 6. Score proprio 0-1000 + alertas
/// 7. Status vira Concluida — corretor vê resultado
/// </summary>
public class AnaliseCreditoService
{
    private readonly ApplicationDbContext _context;
    private readonly IOpenFinanceProvider _provider;
    private readonly ILogger<AnaliseCreditoService> _logger;

    public AnaliseCreditoService(
        ApplicationDbContext context,
        IOpenFinanceProvider provider,
        ILogger<AnaliseCreditoService> logger)
    {
        _context = context;
        _provider = provider;
        _logger = logger;
    }

    /// <summary>
    /// Corretor solicita análise para um cliente. Retorna o link que o
    /// cliente deve acessar para autorizar Open Finance.
    /// </summary>
    public async Task<(SolicitacaoAnaliseCredito solicitacao, string connectUrl)> CriarSolicitacaoAsync(
        int clienteId,
        int? corretorId,
        CancellationToken ct = default)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId, ct)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        var connect = await _provider.CriarConnectLinkAsync(clienteId.ToString(), ct);
        if (!connect.Sucesso || string.IsNullOrEmpty(connect.ConnectUrl))
            throw new InvalidOperationException(connect.MensagemErro ?? "Falha ao criar link.");

        var solicitacao = new SolicitacaoAnaliseCredito
        {
            TenantId = cliente.TenantId,
            ClienteId = cliente.Id,
            SolicitadoPorUsuarioId = corretorId,
            Status = StatusAnaliseCredito.Pendente,
            Token = connect.ConnectToken ?? Guid.NewGuid().ToString("N"),
            Provedor = _provider.Nome,
            ExpiraEm = DateTime.UtcNow.AddMonths(12), // Bacen max
            CreatedAt = DateTime.UtcNow
        };
        _context.SolicitacoesAnaliseCredito.Add(solicitacao);
        await _context.SaveChangesAsync(ct);

        return (solicitacao, connect.ConnectUrl);
    }

    /// <summary>
    /// Processa callback: cliente autorizou, providerItemId disponível,
    /// puxamos dados e calculamos análise.
    ///
    /// Em produção seria chamado pelo webhook do Pluggy. No stub, o
    /// controller pode chamar diretamente para teste.
    /// </summary>
    public async Task<SolicitacaoAnaliseCredito> ProcessarAutorizacaoAsync(
        string token,
        string providerItemId,
        CancellationToken ct = default)
    {
        var sol = await _context.SolicitacoesAnaliseCredito.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Token == token, ct)
            ?? throw new InvalidOperationException("Token inválido.");

        sol.ProviderItemId = providerItemId;
        sol.Status = StatusAnaliseCredito.EmProcessamento;
        sol.ConsentimentoLgpd = true;
        sol.ConsentimentoLgpdEm = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        try
        {
            var dados = await _provider.BuscarDadosAsync(providerItemId, ct);
            if (!dados.Sucesso)
            {
                sol.Status = StatusAnaliseCredito.Falhou;
                sol.MensagemErro = dados.MensagemErro;
                await _context.SaveChangesAsync(ct);
                return sol;
            }

            var analise = Analisar(dados);
            sol.RendaMediaComprovada = analise.Renda;
            sol.VolatilidadeRenda = analise.Volatilidade;
            sol.DividasRecorrentes = analise.Dividas;
            sol.CapacidadePagamento = analise.Capacidade;
            sol.Score = analise.Score;
            sol.AlertasJson = JsonSerializer.Serialize(analise.Alertas);
            sol.Status = StatusAnaliseCredito.Concluida;
            sol.ConcluidaEm = DateTime.UtcNow;

            _logger.LogInformation("Análise crédito {Id} concluída: score={Score} renda={Renda}",
                sol.Id, analise.Score, analise.Renda);
        }
        catch (Exception ex)
        {
            sol.Status = StatusAnaliseCredito.Falhou;
            sol.MensagemErro = ex.Message;
            _logger.LogError(ex, "Falha ao processar análise {Id}", sol.Id);
        }

        await _context.SaveChangesAsync(ct);
        return sol;
    }

    public async Task RevogarAsync(long solicitacaoId, CancellationToken ct = default)
    {
        var sol = await _context.SolicitacoesAnaliseCredito.FirstOrDefaultAsync(s => s.Id == solicitacaoId, ct);
        if (sol is null) return;
        // Zera dados financeiros (LGPD)
        sol.RendaMediaComprovada = null;
        sol.VolatilidadeRenda = null;
        sol.DividasRecorrentes = null;
        sol.CapacidadePagamento = null;
        sol.Score = null;
        sol.AlertasJson = null;
        sol.Status = StatusAnaliseCredito.Revogada;
        await _context.SaveChangesAsync(ct);
    }

    // ========== Motor de análise ==========

    private AnaliseResultado Analisar(OpenFinanceDadosBrutos dados)
    {
        var alertas = new List<string>();

        // Renda: créditos categorizados como "salario" nos últimos 6 meses
        var creditosSalario = dados.Transacoes
            .Where(t => t.Valor > 0 && t.Categoria == "salario")
            .ToList();
        decimal renda = 0;
        decimal volatilidade = 0;

        if (creditosSalario.Count > 0)
        {
            renda = creditosSalario.Average(t => t.Valor);
            if (creditosSalario.Count >= 3)
            {
                var variancia = creditosSalario.Sum(t => (t.Valor - renda) * (t.Valor - renda)) / creditosSalario.Count;
                volatilidade = (decimal)Math.Sqrt((double)variancia);
            }
        }
        else
        {
            alertas.Add("Nenhum crédito categorizado como salário nos últimos 6 meses.");
        }

        if (volatilidade > renda * 0.2m)
        {
            alertas.Add("Renda apresenta alta variabilidade (>20% do valor médio).");
        }

        // Dividas recorrentes: débitos de financiamento/aluguel/cartao por mês
        var debitosRecorrentes = dados.Transacoes
            .Where(t => t.Valor < 0
                     && (t.Categoria == "financiamento" || t.Categoria == "aluguel" || t.Categoria == "cartao"))
            .GroupBy(t => new { t.Data.Year, t.Data.Month, t.Categoria })
            .Select(g => new { Mes = g.Key, Total = g.Sum(x => x.Valor) })
            .ToList();

        var dividasMes = debitosRecorrentes.Count == 0 ? 0
            : debitosRecorrentes.GroupBy(d => new { d.Mes.Year, d.Mes.Month })
                .Average(g => g.Sum(x => Math.Abs(x.Total)));

        if (dividasMes > renda * 0.30m)
        {
            alertas.Add($"Dívidas recorrentes representam {(dividasMes / Math.Max(renda, 1m) * 100m):N1}% da renda.");
        }

        // Padrões suspeitos
        if (dados.Transacoes.Any(t => (t.Descricao ?? "").Contains("APOSTA", StringComparison.OrdinalIgnoreCase)
                                    || (t.Descricao ?? "").Contains("BETANO", StringComparison.OrdinalIgnoreCase)
                                    || (t.Descricao ?? "").Contains("STAKE", StringComparison.OrdinalIgnoreCase)))
        {
            alertas.Add("Detectadas transações relacionadas a apostas.");
        }

        // Capacidade de pagamento (30% da renda líquida - dívidas)
        var capacidade = Math.Max(0, (renda * 0.30m) - dividasMes);

        // Score 0-1000
        var score = CalcularScore(renda, volatilidade, dividasMes, alertas.Count);

        return new AnaliseResultado
        {
            Renda = Math.Round(renda, 2),
            Volatilidade = Math.Round(volatilidade, 2),
            Dividas = Math.Round(dividasMes, 2),
            Capacidade = Math.Round(capacidade, 2),
            Score = score,
            Alertas = alertas
        };
    }

    private static int CalcularScore(decimal renda, decimal volatilidade, decimal dividas, int qtdAlertas)
    {
        if (renda <= 0) return 0;

        var score = 500; // base

        // Renda: até +300 pontos
        score += (int)Math.Min(300, renda / 100m);

        // Estabilidade: até +100 pontos (se volatilidade < 10% da renda)
        var pctVolat = volatilidade / Math.Max(renda, 1m);
        if (pctVolat < 0.10m) score += 100;
        else if (pctVolat < 0.20m) score += 50;

        // Dividas: subtrai (se > 30% renda: -150 pontos)
        var pctDividas = dividas / Math.Max(renda, 1m);
        if (pctDividas > 0.30m) score -= 150;
        else if (pctDividas > 0.20m) score -= 50;

        // Alertas
        score -= qtdAlertas * 30;

        return Math.Clamp(score, 0, 1000);
    }

    private class AnaliseResultado
    {
        public decimal Renda { get; set; }
        public decimal Volatilidade { get; set; }
        public decimal Dividas { get; set; }
        public decimal Capacidade { get; set; }
        public int Score { get; set; }
        public List<string> Alertas { get; set; } = new();
    }
}
