using System.Security.Claims;
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
    public async Task<ActionResult<PagedResult<ClienteResponse>>> GetClientes(
        [FromQuery] PageRequest page,
        [FromQuery] StatusFunil? statusFunil,
        [FromQuery] OrigemLead? origemLead,
        [FromQuery] int? corretorId)
    {
        var (p, s) = page.Normalized();

        var query = _context.Clientes.AsNoTracking().AsQueryable();
        if (statusFunil.HasValue) query = query.Where(c => c.StatusFunil == statusFunil);
        if (origemLead.HasValue) query = query.Where(c => c.OrigemLead == origemLead);
        if (corretorId.HasValue) query = query.Where(c => c.CorretorResponsavelId == corretorId);

        query = query.OrderByDescending(c => c.DataCadastro);
        var total = await query.CountAsync();
        var items = await query.Skip((p - 1) * s).Take(s).ToListAsync();

        return Ok(PagedResult<ClienteResponse>.Create(
            _mapper.Map<List<ClienteResponse>>(items), p, s, total));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteResponse>> GetCliente(int id)
    {
        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null) return NotFound();
        return Ok(_mapper.Map<ClienteResponse>(cliente));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> PostCliente([FromBody] ClienteCreateRequest request)
    {
        var cpf = DocumentosValidator.NormalizarDigitos(request.Cpf);
        if (await _context.Clientes.AnyAsync(c => c.Cpf == cpf))
            return Conflict(new { message = "CPF já cadastrado" });
        if (await _context.Clientes.AnyAsync(c => c.Email == request.Email))
            return Conflict(new { message = "Email já cadastrado" });

        var cliente = _mapper.Map<Cliente>(request);
        cliente.Cpf = cpf;
        cliente.DataCadastro = DateTime.UtcNow;
        cliente.StatusFunil = StatusFunil.Lead;
        if (request.ConsentimentoLgpd) cliente.ConsentimentoLgpdEm = DateTime.UtcNow;

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        // Timeline automática: primeira interação é o próprio cadastro
        _context.HistoricoInteracoes.Add(new HistoricoInteracao
        {
            ClienteId = cliente.Id,
            UsuarioId = GetCurrentUserId(),
            Tipo = TipoInteracao.MensagemInterna,
            Conteudo = "Cliente cadastrado no sistema.",
            DataHora = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, _mapper.Map<ClienteResponse>(cliente));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutCliente(int id, [FromBody] ClienteUpdateRequest request)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null) return NotFound();

        if (await _context.Clientes.AnyAsync(c => c.Email == request.Email && c.Id != id))
            return Conflict(new { message = "Email já cadastrado em outro cliente" });

        var statusAnterior = cliente.StatusFunil;
        cliente.Nome = request.Nome;
        cliente.Rg = request.Rg;
        cliente.DataNascimento = request.DataNascimento;
        cliente.EstadoCivil = request.EstadoCivil;
        cliente.RegimeBens = request.RegimeBens;
        cliente.Profissao = request.Profissao;
        cliente.Empresa = request.Empresa;
        cliente.RendaMensal = request.RendaMensal;
        cliente.Email = request.Email;
        cliente.Telefone = request.Telefone;
        cliente.Whatsapp = request.Whatsapp;
        cliente.OrigemLead = request.OrigemLead;
        cliente.StatusFunil = request.StatusFunil;
        cliente.CorretorResponsavelId = request.CorretorResponsavelId;
        cliente.ConjugeId = request.ConjugeId;
        if (request.Endereco is not null)
        {
            cliente.Endereco = _mapper.Map<Domain.ValueObjects.Endereco>(request.Endereco);
        }

        await _context.SaveChangesAsync();

        if (statusAnterior != request.StatusFunil)
        {
            _context.HistoricoInteracoes.Add(new HistoricoInteracao
            {
                ClienteId = id,
                UsuarioId = GetCurrentUserId(),
                Tipo = TipoInteracao.MensagemInterna,
                Conteudo = $"Status do funil: {statusAnterior} → {request.StatusFunil}.",
                DataHora = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpPost("{id:int}/consentimento-lgpd")]
    public async Task<IActionResult> AtualizarConsentimentoLgpd(int id, [FromBody] ConsentimentoLgpdRequest request)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null) return NotFound();

        cliente.ConsentimentoLgpd = request.Aceitou;
        cliente.ConsentimentoLgpdEm = request.Aceitou ? DateTime.UtcNow : null;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// LGPD Art. 18: direito à portabilidade. Exporta todos os dados do cliente
    /// em JSON. Acessível pelo dono do cadastro (corretor responsável) ou Admin.
    /// </summary>
    [HttpGet("{id:int}/export")]
    public async Task<ActionResult<ClienteLgpdExport>> Export(int id)
    {
        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null) return NotFound();

        var interacoes = await _context.HistoricoInteracoes.AsNoTracking()
            .Include(i => i.Usuario)
            .Where(i => i.ClienteId == id)
            .OrderByDescending(i => i.DataHora)
            .ToListAsync();

        var visitas = await _context.Visitas.AsNoTracking()
            .Include(v => v.Corretor)
            .Include(v => v.Empreendimento)
            .Where(v => v.ClienteId == id)
            .ToListAsync();

        var export = new ClienteLgpdExport
        {
            Cliente = _mapper.Map<ClienteResponse>(cliente),
            Interacoes = _mapper.Map<List<InteracaoResponse>>(interacoes),
            Visitas = _mapper.Map<List<VisitaResponse>>(visitas)
        };

        return Ok(export);
    }

    [HttpGet("{id:int}/interacoes")]
    public async Task<ActionResult<IReadOnlyList<InteracaoResponse>>> GetInteracoes(int id)
    {
        var items = await _context.HistoricoInteracoes.AsNoTracking()
            .Include(i => i.Usuario)
            .Where(i => i.ClienteId == id)
            .OrderByDescending(i => i.DataHora)
            .Take(200)
            .ToListAsync();

        return Ok(_mapper.Map<List<InteracaoResponse>>(items));
    }

    [HttpPost("{id:int}/interacoes")]
    public async Task<ActionResult<InteracaoResponse>> PostInteracao(int id, [FromBody] InteracaoCreateRequest request)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null) return NotFound();

        var interacao = _mapper.Map<HistoricoInteracao>(request);
        interacao.ClienteId = id;
        interacao.UsuarioId = GetCurrentUserId();
        interacao.DataHora = DateTime.UtcNow;

        _context.HistoricoInteracoes.Add(interacao);
        await _context.SaveChangesAsync();

        await _context.Entry(interacao).Reference(i => i.Usuario).LoadAsync();
        return CreatedAtAction(nameof(GetInteracoes), new { id }, _mapper.Map<InteracaoResponse>(interacao));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCliente(int id)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null) return NotFound();

        if (await _context.Vendas.AnyAsync(v => v.ClienteId == id))
            return Conflict(new { message = "Cliente tem vendas associadas" });

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private int? GetCurrentUserId()
    {
        var raw = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(raw, out var id) ? id : null;
    }
}
