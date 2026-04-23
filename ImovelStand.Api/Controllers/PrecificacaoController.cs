using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Abstractions;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;
using ImovelStand.Infrastructure.Precificacao;

namespace ImovelStand.Api.Controllers;

/// <summary>
/// Endpoints da precificação dinâmica:
/// - GET /sugestoes: lista sugestões pendentes (Admin/Gerente)
/// - POST /sugestoes/{id}/aceitar: aceita e aplica preço novo
/// - POST /sugestoes/{id}/rejeitar: rejeita com motivo
/// - POST /calcular/{apartamentoId}: força cálculo de nova sugestão
/// - POST /recalcular-tenant: roda motor para todo o tenant
/// </summary>
[Authorize(Roles = "Admin,Gerente")]
[ApiController]
[Route("api/precificacao")]
public class PrecificacaoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PrecificacaoService _service;
    private readonly ITenantProvider _tenantProvider;

    public PrecificacaoController(
        ApplicationDbContext context,
        PrecificacaoService service,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _service = service;
        _tenantProvider = tenantProvider;
    }

    [HttpGet("sugestoes")]
    public async Task<ActionResult<List<SugestaoPrecoDto>>> Listar(
        [FromQuery] string status = "pendente",
        CancellationToken ct = default)
    {
        var q = _context.SugestoesPreco.AsNoTracking()
            .Include(s => s.Apartamento).ThenInclude(a => a.Torre)
            .Include(s => s.Apartamento).ThenInclude(a => a.Tipologia)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && status != "todas")
        {
            q = q.Where(s => s.Status == status);
        }

        var items = await q
            .OrderByDescending(s => s.Confianca)
            .ThenByDescending(s => Math.Abs((double)s.VariacaoPct))
            .Take(100)
            .Select(s => new SugestaoPrecoDto
            {
                Id = s.Id,
                ApartamentoId = s.ApartamentoId,
                ApartamentoNumero = s.Apartamento.Numero,
                TorreNome = s.Apartamento.Torre != null ? s.Apartamento.Torre.Nome : null,
                TipologiaNome = s.Apartamento.Tipologia != null ? s.Apartamento.Tipologia.Nome : null,
                PrecoAtual = s.PrecoAtual,
                PrecoSugerido = s.PrecoSugerido,
                VariacaoPct = s.VariacaoPct,
                Motivo = s.Motivo,
                Justificativa = s.Justificativa,
                Confianca = s.Confianca,
                VelocidadeVendaSemanal = s.VelocidadeVendaSemanal,
                Status = s.Status,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(ct);

        // Agrega "dinheiro na mesa" (potencial com aumentos pendentes)
        var dinheiroPotencial = items
            .Where(s => s.Status == "pendente" && s.VariacaoPct > 0)
            .Sum(s => s.PrecoSugerido - s.PrecoAtual);

        Response.Headers["X-Dinheiro-Potencial"] = dinheiroPotencial.ToString("F2");

        return Ok(items);
    }

    [HttpPost("sugestoes/{id:long}/aceitar")]
    public async Task<IActionResult> Aceitar(long id, CancellationToken ct)
    {
        var s = await _context.SugestoesPreco
            .Include(x => x.Apartamento)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null) return NotFound();
        if (s.Status != "pendente") return BadRequest(new { message = "Sugestão já foi respondida." });

        var userIdRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdRaw, out var userId);

        // Aplica preço
        s.Apartamento.PrecoAtual = s.PrecoSugerido;
        s.Status = "aceita";
        s.AceitaPorUsuarioId = userId;
        s.RespondidaEm = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Ok(new { novoPreco = s.PrecoSugerido });
    }

    [HttpPost("sugestoes/{id:long}/rejeitar")]
    public async Task<IActionResult> Rejeitar(long id, [FromBody] RejeitarRequest req, CancellationToken ct)
    {
        var s = await _context.SugestoesPreco.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null) return NotFound();
        if (s.Status != "pendente") return BadRequest(new { message = "Sugestão já foi respondida." });

        s.Status = "rejeitada";
        s.MotivoRejeicao = req.Motivo;
        s.RespondidaEm = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("calcular/{apartamentoId:int}")]
    public async Task<ActionResult<SugestaoPrecoDto?>> Calcular(int apartamentoId, CancellationToken ct)
    {
        var sugestao = await _service.CalcularSugestaoAsync(apartamentoId, ct);
        if (sugestao is null) return Ok(null);

        // Persiste a sugestão gerada
        _context.SugestoesPreco.Add(sugestao);
        await _context.SaveChangesAsync(ct);

        return Ok(new SugestaoPrecoDto
        {
            Id = sugestao.Id,
            ApartamentoId = sugestao.ApartamentoId,
            PrecoAtual = sugestao.PrecoAtual,
            PrecoSugerido = sugestao.PrecoSugerido,
            VariacaoPct = sugestao.VariacaoPct,
            Motivo = sugestao.Motivo,
            Justificativa = sugestao.Justificativa,
            Confianca = sugestao.Confianca,
            Status = sugestao.Status,
            CreatedAt = sugestao.CreatedAt
        });
    }

    [HttpPost("recalcular-tenant")]
    public async Task<ActionResult<object>> RecalcularTenant(CancellationToken ct)
    {
        if (!_tenantProvider.HasTenant) return Unauthorized();
        var n = await _service.GerarSugestoesParaTenantAsync(_tenantProvider.TenantId, ct);
        return Ok(new { geradas = n });
    }
}

public class SugestaoPrecoDto
{
    public long Id { get; set; }
    public int ApartamentoId { get; set; }
    public string? ApartamentoNumero { get; set; }
    public string? TorreNome { get; set; }
    public string? TipologiaNome { get; set; }
    public decimal PrecoAtual { get; set; }
    public decimal PrecoSugerido { get; set; }
    public decimal VariacaoPct { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string Justificativa { get; set; } = string.Empty;
    public int Confianca { get; set; }
    public decimal VelocidadeVendaSemanal { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RejeitarRequest
{
    public string? Motivo { get; set; }
}
