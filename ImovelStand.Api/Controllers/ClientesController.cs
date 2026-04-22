using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Infrastructure.Persistence;
using ImovelStand.Domain.Entities;

namespace ImovelStand.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(ApplicationDbContext context, ILogger<ClientesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
    {
        try
        {
            return await _context.Clientes
                .OrderByDescending(c => c.DataCadastro)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar clientes");
            return StatusCode(500, new { message = "Erro ao listar clientes" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Cliente>> GetCliente(int id)
    {
        try
        {
            var cliente = await _context.Clientes
                .Include(c => c.Vendas)
                .Include(c => c.Reservas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound(new { message = "Cliente não encontrado" });
            }

            return cliente;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cliente");
            return StatusCode(500, new { message = "Erro ao buscar cliente" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
    {
        try
        {
            // Verificar se CPF já existe
            if (await _context.Clientes.AnyAsync(c => c.Cpf == cliente.Cpf))
            {
                return BadRequest(new { message = "CPF já cadastrado" });
            }

            // Verificar se Email já existe
            if (await _context.Clientes.AnyAsync(c => c.Email == cliente.Email))
            {
                return BadRequest(new { message = "Email já cadastrado" });
            }

            cliente.DataCadastro = DateTime.UtcNow;
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar cliente");
            return StatusCode(500, new { message = "Erro ao criar cliente" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutCliente(int id, Cliente cliente)
    {
        if (id != cliente.Id)
        {
            return BadRequest(new { message = "ID do cliente não corresponde" });
        }

        try
        {
            // Verificar se CPF já existe em outro cliente
            if (await _context.Clientes.AnyAsync(c => c.Cpf == cliente.Cpf && c.Id != id))
            {
                return BadRequest(new { message = "CPF já cadastrado para outro cliente" });
            }

            // Verificar se Email já existe em outro cliente
            if (await _context.Clientes.AnyAsync(c => c.Email == cliente.Email && c.Id != id))
            {
                return BadRequest(new { message = "Email já cadastrado para outro cliente" });
            }

            _context.Entry(cliente).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ClienteExists(id))
            {
                return NotFound(new { message = "Cliente não encontrado" });
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar cliente");
            return StatusCode(500, new { message = "Erro ao atualizar cliente" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCliente(int id)
    {
        try
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound(new { message = "Cliente não encontrado" });
            }

            // Verificar se há vendas associadas
            if (await _context.Vendas.AnyAsync(v => v.ClienteId == id))
            {
                return BadRequest(new { message = "Não é possível excluir cliente com vendas associadas" });
            }

            // Verificar se há reservas associadas
            if (await _context.Reservas.AnyAsync(r => r.ClienteId == id))
            {
                return BadRequest(new { message = "Não é possível excluir cliente com reservas associadas" });
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir cliente");
            return StatusCode(500, new { message = "Erro ao excluir cliente" });
        }
    }

    private async Task<bool> ClienteExists(int id)
    {
        return await _context.Clientes.AnyAsync(e => e.Id == id);
    }
}
