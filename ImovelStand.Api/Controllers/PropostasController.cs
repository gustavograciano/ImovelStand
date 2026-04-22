using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Common;
using ImovelStand.Application.Dtos;
using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Domain.ValueObjects;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PropostasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly CalculadoraFinanceira _calculadora;

    public PropostasController(ApplicationDbContext context, IMapper mapper, CalculadoraFinanceira calculadora)
    {
        _context = context;
        _mapper = mapper;
        _calculadora = calculadora;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PropostaResponse>>> Listar(
        [FromQuery] PageRequest page,
        [FromQuery] int? apartamentoId,
        [FromQuery] int? clienteId,
        [FromQuery] StatusProposta? status)
    {
        var (p, s) = page.Normalized();

        var query = _context.Propostas.AsNoTracking()
            .Include(pr => pr.Cliente)
            .Include(pr => pr.Apartamento)
            .Include(pr => pr.Corretor)
            .AsQueryable();

        if (apartamentoId.HasValue) query = query.Where(pr => pr.ApartamentoId == apartamentoId);
        if (clienteId.HasValue) query = query.Where(pr => pr.ClienteId == clienteId);
        if (status.HasValue) query = query.Where(pr => pr.Status == status);

        query = query.OrderByDescending(pr => pr.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((p - 1) * s).Take(s).ToListAsync();

        return Ok(PagedResult<PropostaResponse>.Create(
            _mapper.Map<List<PropostaResponse>>(items), p, s, total));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PropostaResponse>> Obter(int id)
    {
        var proposta = await _context.Propostas.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Apartamento)
            .Include(p => p.Corretor)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (proposta is null) return NotFound();
        return Ok(_mapper.Map<PropostaResponse>(proposta));
    }

    [HttpPost]
    public async Task<ActionResult<PropostaResponse>> Criar([FromBody] PropostaCreateRequest request)
    {
        var apartamento = await _context.Apartamentos.FirstOrDefaultAsync(a => a.Id == request.ApartamentoId);
        if (apartamento is null) return BadRequest(new { message = "Apartamento não encontrado." });

        if (apartamento.Status == StatusApartamento.Vendido)
            return Conflict(new { message = "Apartamento já foi vendido." });

        // Conflito: apto já tem proposta Enviada (não Rascunho)
        var haveEnviada = await _context.Propostas.AnyAsync(p =>
            p.ApartamentoId == request.ApartamentoId
            && (p.Status == StatusProposta.Enviada
                || p.Status == StatusProposta.ContrapropostaCliente
                || p.Status == StatusProposta.ContrapropostaCorretor));
        if (haveEnviada)
            return Conflict(new { message = "Já existe proposta ativa para este apartamento." });

        var condicao = _mapper.Map<CondicaoPagamento>(request.Condicao);
        _calculadora.Distribuir(condicao);

        var proposta = new Proposta
        {
            Numero = await GerarNumeroAsync(),
            ClienteId = request.ClienteId,
            ApartamentoId = request.ApartamentoId,
            CorretorId = request.CorretorId,
            ValorOferecido = request.ValorOferecido,
            Status = StatusProposta.Rascunho,
            DataValidade = request.DataValidade,
            Observacoes = request.Observacoes,
            Condicao = condicao,
            CreatedAt = DateTime.UtcNow
        };

        _context.Propostas.Add(proposta);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Obter), new { id = proposta.Id }, _mapper.Map<PropostaResponse>(proposta));
    }

    [HttpPost("{id:int}/enviar")]
    public async Task<IActionResult> Enviar(int id)
    {
        var proposta = await _context.Propostas.FirstOrDefaultAsync(p => p.Id == id);
        if (proposta is null) return NotFound();

        PropostaStateMachine.Garantir(proposta.Status, StatusProposta.Enviada);
        var anterior = proposta.Status;
        proposta.Status = StatusProposta.Enviada;
        proposta.DataEnvio = DateTime.UtcNow;
        if (proposta.DataValidade is null) proposta.DataValidade = DateTime.UtcNow.AddDays(7);

        RegistrarTransicao(proposta, anterior, "Proposta enviada ao cliente.");
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/contrapropor")]
    public async Task<ActionResult<PropostaResponse>> Contrapropor(int id, [FromBody] ContrapropostaRequest request)
    {
        var atual = await _context.Propostas.FirstOrDefaultAsync(p => p.Id == id);
        if (atual is null) return NotFound();

        var destino = request.VemDoCorretor ? StatusProposta.ContrapropostaCorretor : StatusProposta.ContrapropostaCliente;
        PropostaStateMachine.Garantir(atual.Status, destino);

        // Cria nova versão linkada à original, preservando histórico
        var raizId = atual.PropostaOriginalId ?? atual.Id;
        var maxVersao = await _context.Propostas
            .Where(p => p.Id == raizId || p.PropostaOriginalId == raizId)
            .MaxAsync(p => (int?)p.Versao) ?? 1;

        var condicao = _mapper.Map<CondicaoPagamento>(request.Condicao);
        _calculadora.Distribuir(condicao);

        var nova = new Proposta
        {
            Numero = $"{atual.Numero}/v{maxVersao + 1}",
            ClienteId = atual.ClienteId,
            ApartamentoId = atual.ApartamentoId,
            CorretorId = atual.CorretorId,
            PropostaOriginalId = raizId,
            Versao = maxVersao + 1,
            ValorOferecido = request.ValorOferecido,
            Status = destino,
            Observacoes = request.Observacoes,
            Condicao = condicao,
            DataEnvio = DateTime.UtcNow,
            DataValidade = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        RegistrarTransicao(atual, atual.Status, $"Contraproposta criada (v{nova.Versao}).");
        atual.Status = destino;
        atual.UpdatedAt = DateTime.UtcNow;

        _context.Propostas.Add(nova);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Obter), new { id = nova.Id }, _mapper.Map<PropostaResponse>(nova));
    }

    [HttpPost("{id:int}/status")]
    [Authorize(Roles = "Admin,Gerente,Corretor")]
    public async Task<IActionResult> AlterarStatus(int id, [FromBody] AlterarStatusRequest request)
    {
        var proposta = await _context.Propostas.FirstOrDefaultAsync(p => p.Id == id);
        if (proposta is null) return NotFound();

        PropostaStateMachine.Garantir(proposta.Status, request.NovoStatus);
        var anterior = proposta.Status;
        proposta.Status = request.NovoStatus;
        proposta.UpdatedAt = DateTime.UtcNow;
        if (request.NovoStatus is StatusProposta.Aceita or StatusProposta.Reprovada)
            proposta.DataRespostaCliente = DateTime.UtcNow;

        RegistrarTransicao(proposta, anterior, request.Motivo);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string> GerarNumeroAsync()
    {
        var ano = DateTime.UtcNow.Year;
        var count = await _context.Propostas.IgnoreQueryFilters()
            .CountAsync(p => p.CreatedAt.Year == ano);
        return $"PROP-{ano}-{(count + 1):D5}";
    }

    private void RegistrarTransicao(Proposta proposta, StatusProposta anterior, string? motivo)
    {
        var userId = int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : (int?)null;
        _context.PropostaHistoricos.Add(new PropostaHistoricoStatus
        {
            PropostaId = proposta.Id,
            StatusAnterior = anterior,
            StatusNovo = proposta.Status,
            AlteradoPorUsuarioId = userId,
            Motivo = motivo,
            DataAlteracao = DateTime.UtcNow
        });
    }
}
