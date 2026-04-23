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

    // ========== Extrator de proposta (conversa → JSON estruturado) ==========

    public async Task<ExtrairPropostaResponse> ExtrairPropostaAsync(
        int apartamentoId,
        string conversa,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(conversa))
        {
            return new ExtrairPropostaResponse
            {
                Sucesso = false,
                MensagemErro = "Cole o texto da conversa para extrair."
            };
        }

        var apartamento = await _context.Apartamentos.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == apartamentoId, ct);

        if (apartamento is null)
        {
            return new ExtrairPropostaResponse
            {
                Sucesso = false,
                MensagemErro = "Apartamento não encontrado."
            };
        }

        var request = new IARequest
        {
            Operacao = "extrair-proposta",
            PromptVersao = "v1",
            SystemPrompt = PromptLibrary.ExtrairPropostaSystem("v1"),
            UserPrompt = PromptLibrary.ExtrairPropostaUser(conversa, apartamento.PrecoAtual, "v1"),
            MaxTokens = 1500,
            Temperature = 0.1, // preciso, não criativo
            UsarCache = true,
            ExigirJson = true
        };

        var resp = await _ia.InvocarAsync(request, ct);

        if (!resp.Sucesso)
        {
            return new ExtrairPropostaResponse
            {
                Sucesso = false,
                MensagemErro = resp.MensagemErro
            };
        }

        try
        {
            var json = ExtrairJsonPuro(resp.Conteudo);
            var parsed = JsonSerializer.Deserialize<PropostaExtraida>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsed is null)
            {
                return new ExtrairPropostaResponse
                {
                    Sucesso = false,
                    MensagemErro = "Resposta vazia da IA."
                };
            }

            return new ExtrairPropostaResponse
            {
                Sucesso = true,
                Proposta = parsed,
                CustoUsd = resp.CustoUsd,
                InteracaoId = resp.InteracaoId
            };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Extrator: JSON inválido. Conteúdo: {C}",
                resp.Conteudo.Length > 300 ? resp.Conteudo[..300] : resp.Conteudo);
            return new ExtrairPropostaResponse
            {
                Sucesso = false,
                MensagemErro = "Não foi possível interpretar a resposta da IA."
            };
        }
    }

    // ========== Análise de objeções ==========

    public async Task<AnaliseObjecoesResponse> AnalisarObjecoesAsync(
        int clienteId,
        CancellationToken ct = default)
    {
        var cliente = await _context.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clienteId, ct);

        if (cliente is null)
        {
            return new AnaliseObjecoesResponse
            {
                Sucesso = false,
                MensagemErro = "Cliente não encontrado."
            };
        }

        var interacoes = await _context.HistoricoInteracoes.AsNoTracking()
            .Where(i => i.ClienteId == clienteId)
            .OrderByDescending(i => i.DataHora)
            .Take(30)
            .ToListAsync(ct);

        if (interacoes.Count < 2)
        {
            return new AnaliseObjecoesResponse
            {
                Sucesso = true,
                Objecoes = new List<Objecao>(),
                MensagemErro = null
            };
        }

        var sb = new StringBuilder();
        foreach (var i in interacoes.OrderBy(x => x.DataHora))
        {
            var c = i.Conteudo.Length > 300 ? i.Conteudo[..300] + "..." : i.Conteudo;
            sb.AppendLine($"[{i.DataHora:yyyy-MM-dd}] {i.Tipo}: {c}");
        }

        var request = new IARequest
        {
            Operacao = "analisar-objecoes",
            PromptVersao = "v1",
            SystemPrompt = PromptLibrary.AnalisarObjecoesSystem("v1"),
            UserPrompt = PromptLibrary.AnalisarObjecoesUser(sb.ToString(), "v1"),
            MaxTokens = 1200,
            Temperature = 0.3,
            UsarCache = true,
            ExigirJson = true
        };

        var resp = await _ia.InvocarAsync(request, ct);

        if (!resp.Sucesso)
        {
            return new AnaliseObjecoesResponse
            {
                Sucesso = false,
                MensagemErro = resp.MensagemErro,
                Objecoes = new List<Objecao>()
            };
        }

        try
        {
            var json = ExtrairJsonPuro(resp.Conteudo);
            var parsed = JsonSerializer.Deserialize<ObjecoesPayload>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return new AnaliseObjecoesResponse
            {
                Sucesso = true,
                Objecoes = parsed?.Objecoes ?? new List<Objecao>(),
                CustoUsd = resp.CustoUsd,
                DoCache = resp.DoCache,
                InteracaoId = resp.InteracaoId
            };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Objeções: JSON inválido.");
            return new AnaliseObjecoesResponse
            {
                Sucesso = false,
                MensagemErro = "Falha ao interpretar a resposta.",
                Objecoes = new List<Objecao>()
            };
        }
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

public class ExtrairPropostaResponse
{
    public bool Sucesso { get; set; }
    public string? MensagemErro { get; set; }
    public PropostaExtraida? Proposta { get; set; }
    public decimal CustoUsd { get; set; }
    public long InteracaoId { get; set; }
}

public class PropostaExtraida
{
    public decimal ValorOferecido { get; set; }
    public string? Observacoes { get; set; }
    public CondicaoExtraida Condicao { get; set; } = new();
    public List<string> CamposFaltantes { get; set; } = new();
}

public class CondicaoExtraida
{
    public decimal ValorTotal { get; set; }
    public decimal Entrada { get; set; }
    public decimal Sinal { get; set; }
    public int QtdParcelasMensais { get; set; }
    public decimal ValorParcelaMensal { get; set; }
    public int QtdSemestrais { get; set; }
    public decimal ValorSemestral { get; set; }
    public decimal ValorChaves { get; set; }
    public int QtdPosChaves { get; set; }
    public decimal ValorPosChaves { get; set; }
    public string Indice { get; set; } = "SemReajuste";
    public decimal TaxaJurosAnual { get; set; }
}

public class AnaliseObjecoesResponse
{
    public bool Sucesso { get; set; }
    public string? MensagemErro { get; set; }
    public List<Objecao> Objecoes { get; set; } = new();
    public decimal CustoUsd { get; set; }
    public bool DoCache { get; set; }
    public long InteracaoId { get; set; }
}

public class ObjecoesPayload
{
    public List<Objecao> Objecoes { get; set; } = new();
}

public class Objecao
{
    public string Tema { get; set; } = string.Empty;
    public int Ocorrencias { get; set; }
    public string UltimaMencao { get; set; } = string.Empty;
    public string SugestaoContorno { get; set; } = string.Empty;
}
