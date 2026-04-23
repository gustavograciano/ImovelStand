using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Dtos;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TorresController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TorresController> _logger;

    public TorresController(ApplicationDbContext context, ILogger<TorresController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<TorreResponse>>> Listar([FromQuery] int? empreendimentoId, CancellationToken ct)
    {
        var query = _context.Torres.AsNoTracking().Include(t => t.Empreendimento).AsQueryable();
        if (empreendimentoId.HasValue) query = query.Where(t => t.EmpreendimentoId == empreendimentoId);

        var torres = await query.OrderBy(t => t.Nome).ToListAsync(ct);
        var aptosCount = await _context.Apartamentos
            .Where(a => torres.Select(t => t.Id).Contains(a.TorreId))
            .GroupBy(a => a.TorreId)
            .Select(g => new { TorreId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TorreId, x => x.Count, ct);

        return Ok(torres.Select(t => new TorreResponse
        {
            Id = t.Id,
            EmpreendimentoId = t.EmpreendimentoId,
            EmpreendimentoNome = t.Empreendimento?.Nome,
            Nome = t.Nome,
            Pavimentos = t.Pavimentos,
            ApartamentosPorPavimento = t.ApartamentosPorPavimento,
            QtdApartamentos = aptosCount.GetValueOrDefault(t.Id, 0)
        }).ToList());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<ActionResult<TorreResponse>> Criar([FromBody] TorreCreateRequest request, CancellationToken ct)
    {
        var empExists = await _context.Empreendimentos.AnyAsync(e => e.Id == request.EmpreendimentoId, ct);
        if (!empExists) return BadRequest(new { message = "Empreendimento não encontrado." });

        if (await _context.Torres.AnyAsync(t => t.EmpreendimentoId == request.EmpreendimentoId && t.Nome == request.Nome, ct))
            return Conflict(new { message = "Já existe torre com esse nome neste empreendimento." });

        var torre = new Torre
        {
            EmpreendimentoId = request.EmpreendimentoId,
            Nome = request.Nome,
            Pavimentos = request.Pavimentos,
            ApartamentosPorPavimento = request.ApartamentosPorPavimento,
            DataCadastro = DateTime.UtcNow
        };
        _context.Torres.Add(torre);
        await _context.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Listar), new { id = torre.Id }, new TorreResponse
        {
            Id = torre.Id,
            EmpreendimentoId = torre.EmpreendimentoId,
            Nome = torre.Nome,
            Pavimentos = torre.Pavimentos,
            ApartamentosPorPavimento = torre.ApartamentosPorPavimento,
            QtdApartamentos = 0
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] TorreUpdateRequest request, CancellationToken ct)
    {
        var torre = await _context.Torres.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (torre is null) return NotFound();
        torre.Nome = request.Nome;
        torre.Pavimentos = request.Pavimentos;
        torre.ApartamentosPorPavimento = request.ApartamentosPorPavimento;
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        var torre = await _context.Torres.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (torre is null) return NotFound();

        if (await _context.Apartamentos.AnyAsync(a => a.TorreId == id, ct))
            return Conflict(new { message = "Torre tem apartamentos. Remova os apartamentos antes." });

        _context.Torres.Remove(torre);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}
