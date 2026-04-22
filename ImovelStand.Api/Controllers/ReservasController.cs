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
public class ReservasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReservasController> _logger;

    public ReservasController(ApplicationDbContext context, ILogger<ReservasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reserva>>> GetReservas()
    {
        try
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Apartamento)
                .OrderByDescending(r => r.DataReserva)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar reservas");
            return StatusCode(500, new { message = "Erro ao listar reservas" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Reserva>> GetReserva(int id)
    {
        try
        {
            var reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Apartamento)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound(new { message = "Reserva não encontrada" });
            }

            return reserva;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar reserva");
            return StatusCode(500, new { message = "Erro ao buscar reserva" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Reserva>> PostReserva(Reserva reserva)
    {
        try
        {
            // Verificar se o apartamento existe
            var apartamento = await _context.Apartamentos.FindAsync(reserva.ApartamentoId);
            if (apartamento == null)
            {
                return NotFound(new { message = "Apartamento não encontrado" });
            }

            // Verificar se o apartamento está disponível
            if (apartamento.Status != StatusApartamento.Disponivel)
            {
                return BadRequest(new { message = $"Apartamento não está disponível. Status atual: {apartamento.Status}" });
            }

            // Verificar se o cliente existe
            var cliente = await _context.Clientes.FindAsync(reserva.ClienteId);
            if (cliente == null)
            {
                return NotFound(new { message = "Cliente não encontrado" });
            }

            // Verificar se já existe uma reserva ativa para este apartamento
            var reservaExistente = await _context.Reservas
                .AnyAsync(r => r.ApartamentoId == reserva.ApartamentoId && r.Status == "Ativa");

            if (reservaExistente)
            {
                return BadRequest(new { message = "Já existe uma reserva ativa para este apartamento" });
            }

            reserva.DataReserva = DateTime.UtcNow;
            reserva.Status = "Ativa";
            reserva.DataExpiracao = DateTime.UtcNow.AddDays(7); // Reserva válida por 7 dias

            // Atualizar status do apartamento
            apartamento.Status = StatusApartamento.Reservado;

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReserva), new { id = reserva.Id }, reserva);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar reserva");
            return StatusCode(500, new { message = "Erro ao criar reserva" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutReserva(int id, Reserva reserva)
    {
        if (id != reserva.Id)
        {
            return BadRequest(new { message = "ID da reserva não corresponde" });
        }

        try
        {
            var reservaExistente = await _context.Reservas
                .Include(r => r.Apartamento)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservaExistente == null)
            {
                return NotFound(new { message = "Reserva não encontrada" });
            }

            // Se a reserva está sendo cancelada, liberar o apartamento
            if (reserva.Status == "Cancelada" && reservaExistente.Status != "Cancelada")
            {
                reservaExistente.Apartamento.Status = StatusApartamento.Disponivel;
            }

            // Se a reserva está sendo confirmada, manter apartamento reservado
            if (reserva.Status == "Confirmada" && reservaExistente.Status != "Confirmada")
            {
                reservaExistente.Apartamento.Status = StatusApartamento.Reservado;
            }

            reservaExistente.Status = reserva.Status;
            reservaExistente.Observacoes = reserva.Observacoes;
            reservaExistente.DataExpiracao = reserva.DataExpiracao;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ReservaExists(id))
            {
                return NotFound(new { message = "Reserva não encontrada" });
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar reserva");
            return StatusCode(500, new { message = "Erro ao atualizar reserva" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReserva(int id)
    {
        try
        {
            var reserva = await _context.Reservas
                .Include(r => r.Apartamento)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound(new { message = "Reserva não encontrada" });
            }

            // Liberar o apartamento se a reserva estava ativa
            if (reserva.Status == "Ativa" || reserva.Status == "Confirmada")
            {
                reserva.Apartamento.Status = StatusApartamento.Disponivel;
            }

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir reserva");
            return StatusCode(500, new { message = "Erro ao excluir reserva" });
        }
    }

    private async Task<bool> ReservaExists(int id)
    {
        return await _context.Reservas.AnyAsync(e => e.Id == id);
    }
}
