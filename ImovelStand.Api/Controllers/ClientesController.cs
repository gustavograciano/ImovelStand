using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Common;
using ImovelStand.Application.Dtos;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(ApplicationDbContext context, IMapper mapper, ILogger<ClientesController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClienteResponse>>> GetClientes([FromQuery] PageRequest page)
    {
        var (p, s) = page.Normalized();

        var query = _context.Clientes.AsNoTracking().OrderByDescending(c => c.DataCadastro);
        var total = await query.CountAsync();
        var items = await query.Skip((p - 1) * s).Take(s).ToListAsync();

        var response = PagedResult<ClienteResponse>.Create(
            _mapper.Map<List<ClienteResponse>>(items),
            p, s, total);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteResponse>> GetCliente(int id)
    {
        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null) return NotFound(new { message = "Cliente não encontrado" });
        return Ok(_mapper.Map<ClienteResponse>(cliente));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> PostCliente([FromBody] ClienteCreateRequest request)
    {
        var cpfNormalizado = DocumentosValidator.NormalizarDigitos(request.Cpf);

        if (await _context.Clientes.AnyAsync(c => c.Cpf == cpfNormalizado))
            return Conflict(new { message = "CPF já cadastrado" });

        if (await _context.Clientes.AnyAsync(c => c.Email == request.Email))
            return Conflict(new { message = "Email já cadastrado" });

        var cliente = _mapper.Map<Cliente>(request);
        cliente.Cpf = cpfNormalizado;
        cliente.DataCadastro = DateTime.UtcNow;

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<ClienteResponse>(cliente);
        return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutCliente(int id, [FromBody] ClienteUpdateRequest request)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null) return NotFound(new { message = "Cliente não encontrado" });

        if (await _context.Clientes.AnyAsync(c => c.Email == request.Email && c.Id != id))
            return Conflict(new { message = "Email já cadastrado em outro cliente" });

        cliente.Nome = request.Nome;
        cliente.Email = request.Email;
        cliente.Telefone = request.Telefone;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCliente(int id)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null) return NotFound(new { message = "Cliente não encontrado" });

        if (await _context.Vendas.AnyAsync(v => v.ClienteId == id))
            return Conflict(new { message = "Cliente tem vendas associadas" });

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
