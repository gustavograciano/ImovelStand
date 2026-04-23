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
public class TipologiasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TipologiasController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<TipologiaResponse>>> Listar([FromQuery] int? empreendimentoId, CancellationToken ct)
    {
        var query = _context.Tipologias.AsNoTracking().AsQueryable();
        if (empreendimentoId.HasValue) query = query.Where(t => t.EmpreendimentoId == empreendimentoId);
        var items = await query.OrderBy(t => t.Nome).ToListAsync(ct);
        return Ok(items.Select(Map).ToList());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<ActionResult<TipologiaResponse>> Criar([FromBody] TipologiaCreateRequest request, CancellationToken ct)
    {
        if (!await _context.Empreendimentos.AnyAsync(e => e.Id == request.EmpreendimentoId, ct))
            return BadRequest(new { message = "Empreendimento não encontrado." });

        if (await _context.Tipologias.AnyAsync(t => t.EmpreendimentoId == request.EmpreendimentoId && t.Nome == request.Nome, ct))
            return Conflict(new { message = "Já existe tipologia com esse nome neste empreendimento." });

        var tip = new Tipologia
        {
            EmpreendimentoId = request.EmpreendimentoId,
            Nome = request.Nome,
            AreaPrivativa = request.AreaPrivativa,
            AreaTotal = request.AreaTotal,
            Quartos = request.Quartos,
            Suites = request.Suites,
            Banheiros = request.Banheiros,
            Vagas = request.Vagas,
            PrecoBase = request.PrecoBase,
            PlantaUrl = request.PlantaUrl,
            DataCadastro = DateTime.UtcNow
        };
        _context.Tipologias.Add(tip);
        await _context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Listar), new { id = tip.Id }, Map(tip));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] TipologiaUpdateRequest request, CancellationToken ct)
    {
        var tip = await _context.Tipologias.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tip is null) return NotFound();
        tip.Nome = request.Nome;
        tip.AreaPrivativa = request.AreaPrivativa;
        tip.AreaTotal = request.AreaTotal;
        tip.Quartos = request.Quartos;
        tip.Suites = request.Suites;
        tip.Banheiros = request.Banheiros;
        tip.Vagas = request.Vagas;
        tip.PrecoBase = request.PrecoBase;
        tip.PlantaUrl = request.PlantaUrl;
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        var tip = await _context.Tipologias.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tip is null) return NotFound();

        if (await _context.Apartamentos.AnyAsync(a => a.TipologiaId == id, ct))
            return Conflict(new { message = "Tipologia tem apartamentos associados." });

        _context.Tipologias.Remove(tip);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    private static TipologiaResponse Map(Tipologia t) => new()
    {
        Id = t.Id,
        EmpreendimentoId = t.EmpreendimentoId,
        Nome = t.Nome,
        AreaPrivativa = t.AreaPrivativa,
        AreaTotal = t.AreaTotal,
        Quartos = t.Quartos,
        Suites = t.Suites,
        Banheiros = t.Banheiros,
        Vagas = t.Vagas,
        PrecoBase = t.PrecoBase,
        PlantaUrl = t.PlantaUrl
    };
}
