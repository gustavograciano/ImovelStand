using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

/// <summary>
/// Endpoints do Simulador Financeiro.
///
/// /api/simulador/*       → autenticado (corretor usa durante reunião)
/// /api/publico/simular/* → sem auth, rate limit 20/min, captura lead
///                          opcional. Usado pelo widget embedável no site
///                          da incorporadora.
/// </summary>
[ApiController]
public class SimuladorController : ControllerBase
{
    private readonly SimuladorFinanceiroService _sim;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SimuladorController> _logger;

    public SimuladorController(
        SimuladorFinanceiroService sim,
        ApplicationDbContext context,
        ILogger<SimuladorController> logger)
    {
        _sim = sim;
        _context = context;
        _logger = logger;
    }

    // ========== Uso interno (corretor) ==========

    [Authorize]
    [HttpPost("api/simulador")]
    public ActionResult<SimulacaoCompletaResult> Simular([FromBody] SimulacaoCompletaRequest request)
    {
        try
        {
            var result = _sim.SimulacaoCompleta(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ========== API pública (widget embedado) ==========

    /// <summary>
    /// Simulação pública. Sem autenticação, rate limit por IP.
    /// Opcionalmente captura lead (se nome+telefone fornecidos).
    /// TenantSlug no body identifica qual incorporadora receberá o lead.
    /// </summary>
    [AllowAnonymous]
    [EnableRateLimiting("publico")]
    [HttpPost("api/publico/simular")]
    public async Task<ActionResult<SimuladorPublicoResponse>> SimularPublico(
        [FromBody] SimuladorPublicoRequest request,
        CancellationToken ct)
    {
        if (request is null || request.Simulacao is null)
            return BadRequest(new { message = "Request inválido." });

        SimulacaoCompletaResult simResult;
        try
        {
            simResult = _sim.SimulacaoCompleta(request.Simulacao);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        int? leadId = null;

        // Captura de lead (opcional) — só com consentimento explícito
        if (!string.IsNullOrWhiteSpace(request.LeadNome)
            && !string.IsNullOrWhiteSpace(request.LeadTelefone)
            && request.ConsentimentoLgpd
            && !string.IsNullOrWhiteSpace(request.TenantSlug))
        {
            var tenant = await _context.Tenants.AsNoTracking().IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Slug == request.TenantSlug, ct);

            if (tenant is null)
            {
                _logger.LogWarning("Widget simulador: tenant slug {Slug} não encontrado", request.TenantSlug);
            }
            else
            {
                // CPF vazio para leads de simulador — campo obrigatório mas
                // o corretor preenche depois. Usamos placeholder determinístico
                // para não colidir com uniqueness (CPF+TenantId).
                var cpfPlaceholder = $"LEAD-{Guid.NewGuid():N}"[..14];

                var cliente = new Cliente
                {
                    TenantId = tenant.Id,
                    Nome = request.LeadNome,
                    Cpf = cpfPlaceholder,
                    Email = request.LeadEmail ?? $"lead-{cpfPlaceholder}@sem-email.local",
                    Telefone = request.LeadTelefone,
                    OrigemLead = OrigemLead.Site,
                    StatusFunil = StatusFunil.Lead,
                    ConsentimentoLgpd = request.ConsentimentoLgpd,
                    ConsentimentoLgpdEm = DateTime.UtcNow,
                    DataCadastro = DateTime.UtcNow
                };
                _context.Clientes.Add(cliente);

                _context.HistoricoInteracoes.Add(new HistoricoInteracao
                {
                    TenantId = tenant.Id,
                    Cliente = cliente,
                    Tipo = TipoInteracao.MensagemInterna,
                    Conteudo = $"[Simulador] Lead capturado via widget. " +
                               $"Imóvel simulado: R$ {request.Simulacao.ValorImovel:N2} / " +
                               $"entrada: R$ {request.Simulacao.Entrada:N2} / " +
                               $"renda informada: R$ {request.Simulacao.RendaMensal:N2}.",
                    DataHora = DateTime.UtcNow
                });

                await _context.SaveChangesAsync(ct);
                leadId = cliente.Id;
                _logger.LogInformation("Widget simulador: lead capturado {LeadId} para tenant {Slug}",
                    leadId, request.TenantSlug);
            }
        }

        return Ok(new SimuladorPublicoResponse
        {
            Resultado = simResult,
            LeadCapturado = leadId.HasValue,
            LeadId = leadId
        });
    }
}

public class SimuladorPublicoRequest
{
    public SimulacaoCompletaRequest Simulacao { get; set; } = new();
    public string? TenantSlug { get; set; }
    public string? LeadNome { get; set; }
    public string? LeadEmail { get; set; }
    public string? LeadTelefone { get; set; }
    public bool ConsentimentoLgpd { get; set; }
}

public class SimuladorPublicoResponse
{
    public SimulacaoCompletaResult Resultado { get; set; } = new();
    public bool LeadCapturado { get; set; }
    public int? LeadId { get; set; }
}
