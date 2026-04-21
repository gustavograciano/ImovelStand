using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Infrastructure.Persistence;
using ImovelStand.Domain.Entities;

namespace ImovelStand.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VendasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<VendasController> _logger;

    public VendasController(ApplicationDbContext context, ILogger<VendasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Venda>>> GetVendas()
    {
        try
        {
            return await _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Apartamento)
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar vendas");
            return StatusCode(500, new { message = "Erro ao listar vendas" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Venda>> GetVenda(int id)
    {
        try
        {
            var venda = await _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Apartamento)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
            {
                return NotFound(new { message = "Venda não encontrada" });
            }

            return venda;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar venda");
            return StatusCode(500, new { message = "Erro ao buscar venda" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Venda>> PostVenda(Venda venda)
    {
        try
        {
            // Verificar se o apartamento existe
            var apartamento = await _context.Apartamentos.FindAsync(venda.ApartamentoId);
            if (apartamento == null)
            {
                return NotFound(new { message = "Apartamento não encontrado" });
            }

            // Verificar se o apartamento já foi vendido
            if (apartamento.Status == "Vendido")
            {
                return BadRequest(new { message = "Apartamento já foi vendido" });
            }

            // Verificar se o cliente existe
            var cliente = await _context.Clientes.FindAsync(venda.ClienteId);
            if (cliente == null)
            {
                return NotFound(new { message = "Cliente não encontrado" });
            }

            venda.DataVenda = DateTime.UtcNow;
            venda.Status = "Concluída";

            // Atualizar status do apartamento
            apartamento.Status = "Vendido";

            // Cancelar todas as reservas ativas deste apartamento
            var reservasAtivas = await _context.Reservas
                .Where(r => r.ApartamentoId == venda.ApartamentoId && r.Status == "Ativa")
                .ToListAsync();

            foreach (var reserva in reservasAtivas)
            {
                reserva.Status = "Cancelada";
            }

            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVenda), new { id = venda.Id }, venda);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar venda");
            return StatusCode(500, new { message = "Erro ao criar venda" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutVenda(int id, Venda venda)
    {
        if (id != venda.Id)
        {
            return BadRequest(new { message = "ID da venda não corresponde" });
        }

        try
        {
            var vendaExistente = await _context.Vendas
                .Include(v => v.Apartamento)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vendaExistente == null)
            {
                return NotFound(new { message = "Venda não encontrada" });
            }

            // Se a venda está sendo cancelada, liberar o apartamento
            if (venda.Status == "Cancelada" && vendaExistente.Status != "Cancelada")
            {
                vendaExistente.Apartamento.Status = "Disponível";
            }

            vendaExistente.Status = venda.Status;
            vendaExistente.ValorVenda = venda.ValorVenda;
            vendaExistente.ValorEntrada = venda.ValorEntrada;
            vendaExistente.FormaPagamento = venda.FormaPagamento;
            vendaExistente.Observacoes = venda.Observacoes;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await VendaExists(id))
            {
                return NotFound(new { message = "Venda não encontrada" });
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar venda");
            return StatusCode(500, new { message = "Erro ao atualizar venda" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVenda(int id)
    {
        try
        {
            var venda = await _context.Vendas
                .Include(v => v.Apartamento)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
            {
                return NotFound(new { message = "Venda não encontrada" });
            }

            // Liberar o apartamento
            venda.Apartamento.Status = "Disponível";

            _context.Vendas.Remove(venda);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir venda");
            return StatusCode(500, new { message = "Erro ao excluir venda" });
        }
    }

    private async Task<bool> VendaExists(int id)
    {
        return await _context.Vendas.AnyAsync(e => e.Id == id);
    }
}
