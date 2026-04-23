using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

/// <summary>
/// Gestão dos números WhatsApp cadastrados no Meta Cloud API.
/// Apenas Admin/Gerente manipula (Corretor consome mas não cadastra).
/// </summary>
[Authorize(Roles = "Admin,Gerente")]
[ApiController]
[Route("api/whatsapp/numeros")]
public class NumerosWhatsAppController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NumerosWhatsAppController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<NumeroWhatsAppDto>>> Listar(CancellationToken ct)
    {
        var items = await _context.NumerosWhatsApp.AsNoTracking()
            .Include(n => n.Usuario)
            .OrderBy(n => n.OrdemRoundRobin)
            .ThenBy(n => n.Apelido)
            .Select(n => new NumeroWhatsAppDto
            {
                Id = n.Id,
                PhoneNumberId = n.PhoneNumberId,
                NumeroExibicao = n.NumeroExibicao,
                Apelido = n.Apelido,
                UsuarioId = n.UsuarioId,
                UsuarioNome = n.Usuario != null ? n.Usuario.Nome : null,
                OrdemRoundRobin = n.OrdemRoundRobin,
                Ativo = n.Ativo
            })
            .ToListAsync(ct);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<NumeroWhatsAppDto>> Criar([FromBody] NumeroWhatsAppCreateRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.PhoneNumberId) || string.IsNullOrWhiteSpace(req.NumeroExibicao))
            return BadRequest(new { message = "PhoneNumberId e NumeroExibicao obrigatórios." });

        if (await _context.NumerosWhatsApp.IgnoreQueryFilters().AnyAsync(n => n.PhoneNumberId == req.PhoneNumberId, ct))
            return Conflict(new { message = "PhoneNumberId já cadastrado em outro tenant ou neste." });

        var n = new NumeroWhatsApp
        {
            PhoneNumberId = req.PhoneNumberId,
            NumeroExibicao = req.NumeroExibicao,
            Apelido = req.Apelido,
            UsuarioId = req.UsuarioId,
            OrdemRoundRobin = req.OrdemRoundRobin,
            Ativo = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.NumerosWhatsApp.Add(n);
        await _context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Listar), new { id = n.Id }, new NumeroWhatsAppDto
        {
            Id = n.Id,
            PhoneNumberId = n.PhoneNumberId,
            NumeroExibicao = n.NumeroExibicao,
            Apelido = n.Apelido,
            UsuarioId = n.UsuarioId,
            OrdemRoundRobin = n.OrdemRoundRobin,
            Ativo = n.Ativo
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] NumeroWhatsAppUpdateRequest req, CancellationToken ct)
    {
        var n = await _context.NumerosWhatsApp.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (n is null) return NotFound();
        n.NumeroExibicao = req.NumeroExibicao;
        n.Apelido = req.Apelido;
        n.UsuarioId = req.UsuarioId;
        n.OrdemRoundRobin = req.OrdemRoundRobin;
        n.Ativo = req.Ativo;
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        var n = await _context.NumerosWhatsApp.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (n is null) return NotFound();
        _context.NumerosWhatsApp.Remove(n);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}

public class NumeroWhatsAppDto
{
    public int Id { get; set; }
    public string PhoneNumberId { get; set; } = string.Empty;
    public string NumeroExibicao { get; set; } = string.Empty;
    public string? Apelido { get; set; }
    public int? UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
    public int OrdemRoundRobin { get; set; }
    public bool Ativo { get; set; }
}

public class NumeroWhatsAppCreateRequest
{
    public string PhoneNumberId { get; set; } = string.Empty;
    public string NumeroExibicao { get; set; } = string.Empty;
    public string? Apelido { get; set; }
    public int? UsuarioId { get; set; }
    public int OrdemRoundRobin { get; set; }
}

public class NumeroWhatsAppUpdateRequest
{
    public string NumeroExibicao { get; set; } = string.Empty;
    public string? Apelido { get; set; }
    public int? UsuarioId { get; set; }
    public int OrdemRoundRobin { get; set; }
    public bool Ativo { get; set; }
}
