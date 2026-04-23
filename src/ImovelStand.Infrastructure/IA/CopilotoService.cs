using System.Globalization;
using System.Text;
using System.Text.Json;
using ImovelStand.Application.Dtos;
using ImovelStand.Application.Services;
using ImovelStand.Application.Services.Prompts;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImovelStand.Infrastructure.IA;

/// <summary>
/// Copiloto IA para corretor. Orquestra montagem de contexto, chamada LLM
/// via IIAService e parsing de resposta.
///
/// Vive em Infrastructure porque depende diretamente de ApplicationDbContext
/// para montar contexto rico (cliente + interações + propostas + visitas).
/// </summary>
public class CopilotoService
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly ApplicationDbContext _context;
    private readonly IIAService _ia;
    private readonly ILogger<CopilotoService> _logger;

    public CopilotoService(ApplicationDbContext context, IIAService ia, ILogger<CopilotoService> logger)
    {
        _context = context;
        _ia = ia;
        _logger = logger;
    }

    // ========== Briefing de cliente ==========

    public async Task<BriefingResponse> GerarBriefingAsync(int clienteId, CancellationToken ct = default)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Interacoes.OrderByDescending(i => i.DataHora).Take(15))
                .ThenInclude(i => i.Usuario)
            .Include(c => c.Visitas.OrderByDescending(v => v.DataHora).Take(5))
                .ThenInclude(v => v.Empreendimento)
            .Include(c => c.CorretorResponsavel)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clienteId, ct);

        if (cliente is null)
        {
            return new BriefingResponse { Sucesso = false, MensagemErro = "Cliente não encontrado." };
        }

        // Propostas do cliente (query separada para não explodir include tree)
        var propostas = await _context.Propostas.AsNoTracking()
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync(ct);

        var contexto = MontarContextoCliente(cliente, propostas);

        var request = new IARequest
        {
            Operacao = "briefing-cliente",
            PromptVersao = "v1",
            SystemPrompt = PromptLibrary.BriefingClienteSystem("v1"),
            UserPrompt = PromptLibrary.BriefingClienteUser(contexto, "v1"),
            MaxTokens = 400,
            Temperature = 0.3,
            UsarCache = true
        };

        var resp = await _ia.InvocarAsync(request, ct);
        return new BriefingResponse
        {
            Sucesso = resp.Sucesso,
            MensagemErro = resp.MensagemErro,
            Briefing = resp.Conteudo,
            GeradoEm = DateTime.UtcNow,
            CustoUsd = resp.CustoUsd,
            DoCache = resp.DoCache,
            InteracaoId = resp.InteracaoId
        };
    }

    // ========== Próximas ações (fila do corretor) ==========

    public async Task<ProximasAcoesResponse> GerarProximasAcoesAsync(int corretorId, CancellationToken ct = default)
    {
        // Clientes sob responsabilidade + propostas ativas + últimos contatos
        var clientes = await _context.Clientes.AsNoTracking()
            .Where(c => c.CorretorResponsavelId == corretorId
                     && c.StatusFunil != StatusFunil.Venda
                     && c.StatusFunil != StatusFunil.Descarte)
            .OrderByDescending(c => c.DataCadastro)
            .Take(80)
            .ToListAsync(ct);

        if (clientes.Count == 0)
        {
            return new ProximasAcoesResponse
            {
                Sucesso = true,
                Acoes = new List<AcaoSugerida>(),
                GeradoEm = DateTime.UtcNow
            };
        }

        var clienteIds = clientes.Select(c => c.Id).ToList();

        var ultimasInteracoes = await _context.HistoricoInteracoes.AsNoTracking()
            .Where(i => clienteIds.Contains(i.ClienteId))
            .GroupBy(i => i.ClienteId)
            .Select(g => new { ClienteId = g.Key, UltimaEm = g.Max(x => x.DataHora) })
            .ToDictionaryAsync(x => x.ClienteId, x => x.UltimaEm, ct);

        var propostasAtivas = await _context.Propostas.AsNoTracking()
            .Where(p => clienteIds.Contains(p.ClienteId)
                     && (p.Status == StatusProposta.Enviada
                         || p.Status == StatusProposta.ContrapropostaCliente
                         || p.Status == StatusProposta.ContrapropostaCorretor))
            .ToListAsync(ct);

        var sb = new StringBuilder();
        foreach (var c in clientes)
        {
            sb.Append("- ID:").Append(c.Id)
              .Append(" | ").Append(c.Nome)
              .Append(" | status:").Append(c.StatusFunil)
              .Append(" | origem:").Append(c.OrigemLead?.ToString() ?? "-");
            if (ultimasInteracoes.TryGetValue(c.Id, out var ult))
            {
                var dias = (int)(DateTime.UtcNow - ult).TotalDays;
                sb.Append(" | ultima_interacao_ha:").Append(dias).Append("d");
            }
            else
            {
                sb.Append(" | sem_interacoes");
            }
            var prop = propostasAtivas.FirstOrDefault(p => p.ClienteId == c.Id);
            if (prop is not null)
            {
                sb.Append(" | proposta:").Append(prop.Numero).Append("=").Append(prop.Status);
                if (prop.DataValidade.HasValue)
                {
                    var diasValidade = (int)(prop.DataValidade.Value - DateTime.UtcNow).TotalDays;
                    sb.Append(" validade_em:").Append(diasValidade).Append("d");
                }
            }
            sb.AppendLine();
        }

        var request = new IARequest
        {
            Operacao = "proximas-acoes",
            PromptVersao = "v1",
            SystemPrompt = PromptLibrary.ProximasAcoesSystem("v1"),
            UserPrompt = PromptLibrary.ProximasAcoesUser(sb.ToString(), "v1"),
            MaxTokens = 2000,
            Temperature = 0.2,
            UsarCache = false, // tempo-dependente
            ExigirJson = true
        };

        var resp = await _ia.InvocarAsync(request, ct);

        if (!resp.Sucesso)
        {
            return new ProximasAcoesResponse
            {
                Sucesso = false,
                MensagemErro = resp.MensagemErro,
                Acoes = new List<AcaoSugerida>()
            };
        }

        List<AcaoSugerida> acoes;
        try
        {
            // Prompt pede "apenas JSON" — mas modelos às vezes embrulham em code fence
            var conteudo = ExtrairJsonPuro(resp.Conteudo);
            acoes = JsonSerializer.Deserialize<List<AcaoSugerida>>(conteudo, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<AcaoSugerida>();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "CopilotoService: falha ao parsear próximas ações. Conteúdo: {C}",
                resp.Conteudo.Length > 300 ? resp.Conteudo[..300] : resp.Conteudo);
            return new ProximasAcoesResponse
            {
                Sucesso = false,
                MensagemErro = "Não foi possível interpretar a resposta da IA.",
                Acoes = new List<AcaoSugerida>()
            };
        }

        return new ProximasAcoesResponse
        {
            Sucesso = true,
            Acoes = acoes,
            GeradoEm = DateTime.UtcNow,
            CustoUsd = resp.CustoUsd,
            InteracaoId = resp.InteracaoId
        };
    }

    // ========== Helpers ==========

    private static string MontarContextoCliente(Cliente c, List<Proposta> propostas)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Nome: {c.Nome}");
        sb.AppendLine($"CPF: {c.Cpf}");
        if (c.DataNascimento.HasValue)
        {
            var idade = DateTime.UtcNow.Year - c.DataNascimento.Value.Year;
            sb.AppendLine($"Idade aprox: {idade} anos");
        }
        if (!string.IsNullOrWhiteSpace(c.Profissao)) sb.AppendLine($"Profissão: {c.Profissao}");
        if (!string.IsNullOrWhiteSpace(c.Empresa)) sb.AppendLine($"Empresa: {c.Empresa}");
        if (c.RendaMensal.HasValue) sb.AppendLine($"Renda mensal: {c.RendaMensal.Value.ToString("C", PtBr)}");
        if (c.EstadoCivil.HasValue) sb.AppendLine($"Estado civil: {c.EstadoCivil}");
        sb.AppendLine($"Origem do lead: {c.OrigemLead?.ToString() ?? "-"}");
        sb.AppendLine($"Status no funil: {c.StatusFunil}");
        sb.AppendLine($"Data de cadastro: {c.DataCadastro:yyyy-MM-dd}");
        if (c.CorretorResponsavel is not null)
        {
            sb.AppendLine($"Corretor responsável: {c.CorretorResponsavel.Nome}");
        }

        sb.AppendLine();
        sb.AppendLine($"Visitas ({c.Visitas.Count}):");
        foreach (var v in c.Visitas.Take(5))
        {
            sb.AppendLine($"  - {v.DataHora:yyyy-MM-dd} em {v.Empreendimento?.Nome ?? "?"}; gerou proposta: {v.GerouProposta}");
        }

        sb.AppendLine();
        sb.AppendLine($"Últimas interações ({c.Interacoes.Count}):");
        foreach (var i in c.Interacoes.OrderByDescending(x => x.DataHora).Take(10))
        {
            var conteudo = i.Conteudo.Length > 200 ? i.Conteudo[..200] + "..." : i.Conteudo;
            sb.AppendLine($"  - [{i.DataHora:yyyy-MM-dd}] {i.Tipo} por {i.Usuario?.Nome ?? "sistema"}: {conteudo}");
        }

        if (propostas.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"Propostas ({propostas.Count}):");
            foreach (var p in propostas)
            {
                sb.AppendLine($"  - {p.Numero} v{p.Versao} | {p.Status} | valor: {p.ValorOferecido.ToString("C", PtBr)} | criada: {p.CreatedAt:yyyy-MM-dd}");
            }
        }

        return sb.ToString();
    }

    private static string ExtrairJsonPuro(string texto)
    {
        var t = texto.Trim();
        // Remove ```json ... ``` se existir
        if (t.StartsWith("```"))
        {
            var primeira = t.IndexOf('\n');
            if (primeira > 0) t = t[(primeira + 1)..];
            var ultima = t.LastIndexOf("```", StringComparison.Ordinal);
            if (ultima > 0) t = t[..ultima];
        }
        return t.Trim();
    }
}

// ========== DTOs retornados ao controller ==========

public class BriefingResponse
{
    public bool Sucesso { get; set; }
    public string? MensagemErro { get; set; }
    public string Briefing { get; set; } = string.Empty;
    public DateTime GeradoEm { get; set; }
    public decimal CustoUsd { get; set; }
    public bool DoCache { get; set; }
    public long InteracaoId { get; set; }
}

public class ProximasAcoesResponse
{
    public bool Sucesso { get; set; }
    public string? MensagemErro { get; set; }
    public List<AcaoSugerida> Acoes { get; set; } = new();
    public DateTime GeradoEm { get; set; }
    public decimal CustoUsd { get; set; }
    public long InteracaoId { get; set; }
}

public class AcaoSugerida
{
    public int ClienteId { get; set; }
    public string Prioridade { get; set; } = "media"; // urgente | alta | media
    public string Acao { get; set; } = string.Empty;
    public string Justificativa { get; set; } = string.Empty;
}
