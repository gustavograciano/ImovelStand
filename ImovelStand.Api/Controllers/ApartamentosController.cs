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
public class ApartamentosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ApartamentosController> _logger;

    public ApartamentosController(ApplicationDbContext context, IMapper mapper, ILogger<ApartamentosController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ApartamentoResponse>>> GetApartamentos(
        [FromQuery] PageRequest page,
        [FromQuery] ApartamentoFiltro filtro)
    {
        var (p, s) = page.Normalized();
        var query = _context.Apartamentos
            .Include(a => a.Torre)
            .Include(a => a.Tipologia)
            .AsNoTracking()
            .AsQueryable();

        if (filtro.Status.HasValue) query = query.Where(a => a.Status == filtro.Status);
        if (filtro.TorreId.HasValue) query = query.Where(a => a.TorreId == filtro.TorreId);
        if (filtro.TipologiaId.HasValue) query = query.Where(a => a.TipologiaId == filtro.TipologiaId);
        if (filtro.PavimentoMin.HasValue) query = query.Where(a => a.Pavimento >= filtro.PavimentoMin);
        if (filtro.PavimentoMax.HasValue) query = query.Where(a => a.Pavimento <= filtro.PavimentoMax);
        if (filtro.PrecoMin.HasValue) query = query.Where(a => a.PrecoAtual >= filtro.PrecoMin);
        if (filtro.PrecoMax.HasValue) query = query.Where(a => a.PrecoAtual <= filtro.PrecoMax);

        query = query.OrderBy(a => a.TorreId).ThenBy(a => a.Pavimento).ThenBy(a => a.Numero);

        var total = await query.CountAsync();
        var items = await query.Skip((p - 1) * s).Take(s).ToListAsync();

        var response = PagedResult<ApartamentoResponse>.Create(
            _mapper.Map<List<ApartamentoResponse>>(items),
            p, s, total);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApartamentoResponse>> GetApartamento(int id)
    {
        var apartamento = await _context.Apartamentos
            .Include(a => a.Torre)
            .Include(a => a.Tipologia)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (apartamento is null) return NotFound(new { message = "Apartamento não encontrado" });
        return Ok(_mapper.Map<ApartamentoResponse>(apartamento));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Gerente")]
    [Authorization.RequiresPlan(limit: "unidades")]
    public async Task<ActionResult<ApartamentoResponse>> PostApartamento([FromBody] ApartamentoCreateRequest request)
    {
        var torre = await _context.Torres.FirstOrDefaultAsync(t => t.Id == request.TorreId);
        if (torre is null) return BadRequest(new { message = "Torre não encontrada" });

        var tipologia = await _context.Tipologias.FirstOrDefaultAsync(t => t.Id == request.TipologiaId);
        if (tipologia is null) return BadRequest(new { message = "Tipologia não encontrada" });

        if (await _context.Apartamentos.AnyAsync(a => a.TorreId == request.TorreId && a.Numero == request.Numero))
            return Conflict(new { message = "Já existe apartamento com esse número na torre" });

        var apartamento = _mapper.Map<Apartamento>(request);
        apartamento.Status = StatusApartamento.Disponivel;
        apartamento.DataCadastro = DateTime.UtcNow;

        _context.Apartamentos.Add(apartamento);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<ApartamentoResponse>(apartamento);
        return CreatedAtAction(nameof(GetApartamento), new { id = apartamento.Id }, response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<IActionResult> PutApartamento(int id, [FromBody] ApartamentoUpdateRequest request)
    {
        var apartamento = await _context.Apartamentos.FirstOrDefaultAsync(a => a.Id == id);
        if (apartamento is null) return NotFound(new { message = "Apartamento não encontrado" });

        if (await _context.Apartamentos.AnyAsync(a =>
                a.TorreId == apartamento.TorreId && a.Numero == request.Numero && a.Id != id))
            return Conflict(new { message = "Já existe apartamento com esse número na torre" });

        apartamento.TipologiaId = request.TipologiaId;
        apartamento.Numero = request.Numero;
        apartamento.Pavimento = request.Pavimento;
        apartamento.Orientacao = request.Orientacao;
        apartamento.PrecoAtual = request.PrecoAtual;
        apartamento.Status = request.Status;
        apartamento.Observacoes = request.Observacoes;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteApartamento(int id)
    {
        var apartamento = await _context.Apartamentos.FirstOrDefaultAsync(a => a.Id == id);
        if (apartamento is null) return NotFound(new { message = "Apartamento não encontrado" });

        if (await _context.Vendas.AnyAsync(v => v.ApartamentoId == id))
            return Conflict(new { message = "Não é possível excluir apartamento com vendas associadas" });

        if (await _context.Reservas.AnyAsync(r => r.ApartamentoId == id))
            return Conflict(new { message = "Não é possível excluir apartamento com reservas associadas" });

        _context.Apartamentos.Remove(apartamento);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
