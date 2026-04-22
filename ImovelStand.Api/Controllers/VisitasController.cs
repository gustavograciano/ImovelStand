using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Common;
using ImovelStand.Application.Dtos;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VisitasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public VisitasController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<VisitaResponse>>> GetVisitas(
        [FromQuery] PageRequest page,
        [FromQuery] int? clienteId,
        [FromQuery] int? corretorId,
        [FromQuery] int? empreendimentoId)
    {
        var (p, s) = page.Normalized();

        var query = _context.Visitas.AsNoTracking()
            .Include(v => v.Cliente)
            .Include(v => v.Corretor)
            .Include(v => v.Empreendimento)
            .AsQueryable();

        if (clienteId.HasValue) query = query.Where(v => v.ClienteId == clienteId);
        if (corretorId.HasValue) query = query.Where(v => v.CorretorId == corretorId);
        if (empreendimentoId.HasValue) query = query.Where(v => v.EmpreendimentoId == empreendimentoId);

        query = query.OrderByDescending(v => v.DataHora);
        var total = await query.CountAsync();
        var items = await query.Skip((p - 1) * s).Take(s).ToListAsync();

        return Ok(PagedResult<VisitaResponse>.Create(
            _mapper.Map<List<VisitaResponse>>(items), p, s, total));
    }

    [HttpPost]
    public async Task<ActionResult<VisitaResponse>> PostVisita([FromBody] VisitaCreateRequest request)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == request.ClienteId);
        if (cliente is null) return BadRequest(new { message = "Cliente não encontrado" });

        var visita = _mapper.Map<Visita>(request);
        _context.Visitas.Add(visita);

        // Move o cliente pra StatusFunil.Visita se ainda estiver em Lead/Contato
        if (cliente.StatusFunil is StatusFunil.Lead or StatusFunil.Contato)
        {
            cliente.StatusFunil = StatusFunil.Visita;
        }

        await _context.SaveChangesAsync();
        await _context.Entry(visita).Reference(v => v.Cliente).LoadAsync();
        await _context.Entry(visita).Reference(v => v.Corretor).LoadAsync();
        await _context.Entry(visita).Reference(v => v.Empreendimento).LoadAsync();

        return CreatedAtAction(nameof(GetVisitas), null, _mapper.Map<VisitaResponse>(visita));
    }
}
