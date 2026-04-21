using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Infrastructure.Persistence;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;

namespace ImovelStand.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ApartamentosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApartamentosController> _logger;

    public ApartamentosController(ApplicationDbContext context, ILogger<ApartamentosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Apartamento>>> GetApartamentos([FromQuery] string? status = null)
    {
        try
        {
            var query = _context.Apartamentos.AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<StatusApartamento>(status, ignoreCase: true, out var statusEnum))
            {
                query = query.Where(a => a.Status == statusEnum);
            }

            return await query.OrderBy(a => a.Numero).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar apartamentos");
            return StatusCode(500, new { message = "Erro ao listar apartamentos" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Apartamento>> GetApartamento(int id)
    {
        try
        {
            var apartamento = await _context.Apartamentos
                .Include(a => a.Vendas)
                .Include(a => a.Reservas)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apartamento == null)
            {
                return NotFound(new { message = "Apartamento não encontrado" });
            }

            return apartamento;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar apartamento");
            return StatusCode(500, new { message = "Erro ao buscar apartamento" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Apartamento>> PostApartamento(Apartamento apartamento)
    {
        try
        {
            if (await _context.Apartamentos.AnyAsync(a => a.Numero == apartamento.Numero))
            {
                return BadRequest(new { message = "Número de apartamento já cadastrado" });
            }

            apartamento.DataCadastro = DateTime.UtcNow;
            apartamento.Status = StatusApartamento.Disponivel;
            _context.Apartamentos.Add(apartamento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetApartamento), new { id = apartamento.Id }, apartamento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar apartamento");
            return StatusCode(500, new { message = "Erro ao criar apartamento" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutApartamento(int id, Apartamento apartamento)
    {
        if (id != apartamento.Id)
        {
            return BadRequest(new { message = "ID do apartamento não corresponde" });
        }

        try
        {
            if (await _context.Apartamentos.AnyAsync(a => a.Numero == apartamento.Numero && a.Id != id))
            {
                return BadRequest(new { message = "Número de apartamento já cadastrado" });
            }

            _context.Entry(apartamento).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ApartamentoExists(id))
            {
                return NotFound(new { message = "Apartamento não encontrado" });
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar apartamento");
            return StatusCode(500, new { message = "Erro ao atualizar apartamento" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApartamento(int id)
    {
        try
        {
            var apartamento = await _context.Apartamentos.FindAsync(id);
            if (apartamento == null)
            {
                return NotFound(new { message = "Apartamento não encontrado" });
            }

            if (await _context.Vendas.AnyAsync(v => v.ApartamentoId == id))
            {
                return BadRequest(new { message = "Não é possível excluir apartamento com vendas associadas" });
            }

            if (await _context.Reservas.AnyAsync(r => r.ApartamentoId == id))
            {
                return BadRequest(new { message = "Não é possível excluir apartamento com reservas associadas" });
            }

            _context.Apartamentos.Remove(apartamento);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir apartamento");
            return StatusCode(500, new { message = "Erro ao excluir apartamento" });
        }
    }

    private async Task<bool> ApartamentoExists(int id)
    {
        return await _context.Apartamentos.AnyAsync(e => e.Id == id);
    }
}
