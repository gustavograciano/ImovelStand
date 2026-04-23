using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Api.Authorization;
using ImovelStand.Application.Common;
using ImovelStand.Application.Dtos;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmpreendimentosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<EmpreendimentosController> _logger;

    public EmpreendimentosController(ApplicationDbContext context, IMapper mapper, ILogger<EmpreendimentosController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmpreendimentoResponse>>> Listar(CancellationToken ct)
    {
        var items = await _context.Empreendimentos
            .AsNoTracking()
            .OrderBy(e => e.Nome)
            .ToListAsync(ct);
        return Ok(_mapper.Map<List<EmpreendimentoResponse>>(items));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmpreendimentoResponse>> Obter(int id, CancellationToken ct)
    {
        var emp = await _context.Empreendimentos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
        if (emp is null) return NotFound();
        return Ok(_mapper.Map<EmpreendimentoResponse>(emp));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Gerente")]
    [RequiresPlan(limit: "empreendimentos")]
    public async Task<ActionResult<EmpreendimentoResponse>> Criar([FromBody] EmpreendimentoCreateRequest request, CancellationToken ct)
    {
        if (await _context.Empreendimentos.AnyAsync(e => e.Slug == request.Slug, ct))
            return Conflict(new { message = "Slug já existe neste tenant." });

        var emp = _mapper.Map<Empreendimento>(request);
        emp.DataCadastro = DateTime.UtcNow;

        _context.Empreendimentos.Add(emp);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Empreendimento {Slug} criado", emp.Slug);
        return CreatedAtAction(nameof(Obter), new { id = emp.Id }, _mapper.Map<EmpreendimentoResponse>(emp));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] EmpreendimentoUpdateRequest request, CancellationToken ct)
    {
        var emp = await _context.Empreendimentos.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (emp is null) return NotFound();

        _mapper.Map(request, emp);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        var emp = await _context.Empreendimentos.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (emp is null) return NotFound();

        if (await _context.Torres.AnyAsync(t => t.EmpreendimentoId == id, ct))
            return Conflict(new { message = "Empreendimento tem torres associadas. Remova as torres primeiro." });

        _context.Empreendimentos.Remove(emp);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}
