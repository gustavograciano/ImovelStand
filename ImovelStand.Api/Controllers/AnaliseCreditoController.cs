using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.OpenFinance;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

/// <summary>
/// Endpoints de análise de crédito via Open Finance.
/// - POST /clientes/{id}/solicitar: cria solicitação + retorna link
/// - GET /clientes/{id}: lista solicitações do cliente
/// - GET /{id}: detalhe com score e alertas
/// - POST /{id}/autorizar-stub: simula callback do provider (só dev)
/// - POST /{id}/revogar: expurga dados sensíveis (LGPD)
/// </summary>
[Authorize]
[ApiController]
[Route("api/analise-credito")]
public class AnaliseCreditoController : ControllerBase
{
    private readonly AnaliseCreditoService _service;
    private readonly ApplicationDbContext _context;

    public AnaliseCreditoController(AnaliseCreditoService service, ApplicationDbContext context)
    {
        _service = service;
        _context = context;
    }

    [HttpPost("clientes/{clienteId:int}/solicitar")]
    public async Task<ActionResult<object>> Solicitar(int clienteId, CancellationToken ct)
    {
        var userIdRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdRaw, out var userId);

        try
        {
            var (sol, url) = await _service.CriarSolicitacaoAsync(clienteId, userId, ct);
            return Ok(new
            {
                id = sol.Id,
                token = sol.Token,
                connectUrl = url,
                status = sol.Status.ToString()
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("clientes/{clienteId:int}")]
    public async Task<ActionResult<List<AnaliseCreditoDto>>> Listar(int clienteId, CancellationToken ct)
    {
        var items = await _context.SolicitacoesAnaliseCredito.AsNoTracking()
            .Where(s => s.ClienteId == clienteId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        return Ok(items.Select(Map).ToList());
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<AnaliseCreditoDto>> Obter(long id, CancellationToken ct)
    {
        var s = await _context.SolicitacoesAnaliseCredito.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null) return NotFound();
        return Ok(Map(s));
    }

    [HttpPost("{id:long}/autorizar-stub")]
    public async Task<ActionResult<AnaliseCreditoDto>> AutorizarStub(long id, CancellationToken ct)
    {
        var s = await _context.SolicitacoesAnaliseCredito.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null) return NotFound();

        var processada = await _service.ProcessarAutorizacaoAsync(
            s.Token,
            $"stub-item-{s.Id}",
            ct);

        return Ok(Map(processada));
    }

    [HttpPost("{id:long}/revogar")]
    public async Task<IActionResult> Revogar(long id, CancellationToken ct)
    {
        await _service.RevogarAsync(id, ct);
        return NoContent();
    }

    private static AnaliseCreditoDto Map(SolicitacaoAnaliseCredito s)
    {
        List<string> alertas = new();
        if (!string.IsNullOrEmpty(s.AlertasJson))
        {
            try
            {
                alertas = JsonSerializer.Deserialize<List<string>>(s.AlertasJson) ?? new();
            }
            catch { /* ignore */ }
        }

        return new AnaliseCreditoDto
        {
            Id = s.Id,
            ClienteId = s.ClienteId,
            Status = s.Status.ToString(),
            Provedor = s.Provedor,
            RendaMediaComprovada = s.RendaMediaComprovada,
            VolatilidadeRenda = s.VolatilidadeRenda,
            DividasRecorrentes = s.DividasRecorrentes,
            CapacidadePagamento = s.CapacidadePagamento,
            Score = s.Score,
            Alertas = alertas,
            ConsentimentoLgpd = s.ConsentimentoLgpd,
            ConsentimentoLgpdEm = s.ConsentimentoLgpdEm,
            ExpiraEm = s.ExpiraEm,
            ConcluidaEm = s.ConcluidaEm,
            MensagemErro = s.MensagemErro,
            CreatedAt = s.CreatedAt
        };
    }
}

public class AnaliseCreditoDto
{
    public long Id { get; set; }
    public int ClienteId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Provedor { get; set; } = string.Empty;
    public decimal? RendaMediaComprovada { get; set; }
    public decimal? VolatilidadeRenda { get; set; }
    public decimal? DividasRecorrentes { get; set; }
    public decimal? CapacidadePagamento { get; set; }
    public int? Score { get; set; }
    public List<string> Alertas { get; set; } = new();
    public bool ConsentimentoLgpd { get; set; }
    public DateTime? ConsentimentoLgpdEm { get; set; }
    public DateTime ExpiraEm { get; set; }
    public DateTime? ConcluidaEm { get; set; }
    public string? MensagemErro { get; set; }
    public DateTime CreatedAt { get; set; }
}
