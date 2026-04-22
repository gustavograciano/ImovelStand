using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Common;
using ImovelStand.Application.Dtos;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Domain.ValueObjects;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VendasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<VendasController> _logger;

    public VendasController(ApplicationDbContext context, IMapper mapper, ILogger<VendasController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<VendaResponse>>> Listar(
        [FromQuery] PageRequest page,
        [FromQuery] StatusVenda? status,
        [FromQuery] int? corretorId,
        [FromQuery] int? clienteId)
    {
        var (p, s) = page.Normalized();

        var query = _context.Vendas.AsNoTracking()
            .Include(v => v.Cliente)
            .Include(v => v.Apartamento)
            .Include(v => v.Corretor)
            .Include(v => v.Comissoes).ThenInclude(c => c.Usuario)
            .AsQueryable();

        if (status.HasValue) query = query.Where(v => v.Status == status);
        if (corretorId.HasValue) query = query.Where(v => v.CorretorId == corretorId);
        if (clienteId.HasValue) query = query.Where(v => v.ClienteId == clienteId);

        query = query.OrderByDescending(v => v.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((p - 1) * s).Take(s).ToListAsync();

        return Ok(PagedResult<VendaResponse>.Create(
            _mapper.Map<List<VendaResponse>>(items), p, s, total));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VendaResponse>> Obter(int id)
    {
        var venda = await _context.Vendas.AsNoTracking()
            .Include(v => v.Cliente)
            .Include(v => v.Apartamento)
            .Include(v => v.Corretor)
            .Include(v => v.Comissoes).ThenInclude(c => c.Usuario)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (venda is null) return NotFound();
        return Ok(_mapper.Map<VendaResponse>(venda));
    }

    [HttpPost]
    public async Task<ActionResult<VendaResponse>> Criar([FromBody] VendaCreateRequest request)
    {
        var apto = await _context.Apartamentos.FirstOrDefaultAsync(a => a.Id == request.ApartamentoId);
        if (apto is null) return BadRequest(new { message = "Apartamento não encontrado" });
        if (apto.Status == StatusApartamento.Vendido)
            return Conflict(new { message = "Apartamento já foi vendido" });

        var corretor = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == request.CorretorId);
        if (corretor is null) return BadRequest(new { message = "Corretor não encontrado" });

        var numero = await GerarNumeroAsync();
        var venda = new Venda
        {
            Numero = numero,
            PropostaId = request.PropostaId,
            ClienteId = request.ClienteId,
            ApartamentoId = request.ApartamentoId,
            CorretorId = request.CorretorId,
            CorretorCaptacaoId = request.CorretorCaptacaoId,
            ValorFinal = request.ValorFinal,
            Status = StatusVenda.Negociada,
            Observacoes = request.Observacoes,
            CondicaoFinal = _mapper.Map<CondicaoPagamento>(request.CondicaoFinal),
            DataFechamento = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Comissões automáticas baseadas em PercentualComissao dos corretores
        var comissoes = new List<Comissao>();
        if (corretor.PercentualComissao is { } pctVenda && pctVenda > 0)
        {
            comissoes.Add(new Comissao
            {
                UsuarioId = corretor.Id,
                Tipo = TipoComissao.Venda,
                Percentual = pctVenda,
                Valor = Math.Round(request.ValorFinal * pctVenda, 2),
                Status = StatusComissao.Pendente
            });
        }

        if (request.CorretorCaptacaoId is { } captId && captId != corretor.Id)
        {
            var captacao = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == captId);
            if (captacao?.PercentualComissao is { } pctCapt && pctCapt > 0)
            {
                comissoes.Add(new Comissao
                {
                    UsuarioId = captId,
                    Tipo = TipoComissao.Captacao,
                    Percentual = pctCapt,
                    Valor = Math.Round(request.ValorFinal * pctCapt, 2),
                    Status = StatusComissao.Pendente
                });
            }
        }

        venda.Comissoes = comissoes;

        _context.Vendas.Add(venda);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Obter), new { id = venda.Id }, _mapper.Map<VendaResponse>(venda));
    }

    /// <summary>
    /// Workflow crítico: Gerente aprova a venda → transação atômica marca apto
    /// como Vendido, cancela reservas/propostas concorrentes, registra aprovador.
    /// </summary>
    [HttpPost("{id:int}/aprovar")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<IActionResult> Aprovar(int id)
    {
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var venda = await _context.Vendas
                .Include(v => v.Apartamento)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (venda is null) return NotFound();

            if (venda.Status != StatusVenda.Negociada)
                return Conflict(new { message = $"Venda não está em Negociada (status atual: {venda.Status})" });

            if (venda.Apartamento.Status == StatusApartamento.Vendido)
                return Conflict(new { message = "Apartamento já vendido em outra venda" });

            var userId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : (int?)null;

            venda.Status = StatusVenda.EmContrato;
            venda.DataAprovacao = DateTime.UtcNow;
            venda.GerenteAprovadorId = userId;
            venda.Apartamento.Status = StatusApartamento.Vendido;
            venda.UpdatedAt = DateTime.UtcNow;

            // Cancela reservas ativas do apto
            var reservas = await _context.Reservas
                .Where(r => r.ApartamentoId == venda.ApartamentoId && r.Status == "Ativa")
                .ToListAsync();
            foreach (var r in reservas) r.Status = "Cancelada";

            // Cancela propostas ativas no apto (menos esta venda, se veio de Proposta)
            var propostasAtivas = await _context.Propostas
                .Where(p => p.ApartamentoId == venda.ApartamentoId
                    && (p.Status == StatusProposta.Enviada
                        || p.Status == StatusProposta.ContrapropostaCliente
                        || p.Status == StatusProposta.ContrapropostaCorretor)
                    && p.Id != venda.PropostaId)
                .ToListAsync();
            foreach (var prop in propostasAtivas)
            {
                var anterior = prop.Status;
                prop.Status = StatusProposta.Cancelada;
                prop.UpdatedAt = DateTime.UtcNow;
                _context.PropostaHistoricos.Add(new PropostaHistoricoStatus
                {
                    PropostaId = prop.Id,
                    StatusAnterior = anterior,
                    StatusNovo = StatusProposta.Cancelada,
                    AlteradoPorUsuarioId = userId,
                    Motivo = $"Cancelada automaticamente pela aprovação da venda {venda.Numero}.",
                    DataAlteracao = DateTime.UtcNow
                });
            }

            // Se veio de Proposta, marca ela como Aceita
            if (venda.PropostaId is { } propostaVendaId)
            {
                var propostaVenda = await _context.Propostas.FirstOrDefaultAsync(p => p.Id == propostaVendaId);
                if (propostaVenda is not null && propostaVenda.Status != StatusProposta.Aceita)
                {
                    var anterior = propostaVenda.Status;
                    propostaVenda.Status = StatusProposta.Aceita;
                    propostaVenda.DataRespostaCliente = DateTime.UtcNow;
                    propostaVenda.UpdatedAt = DateTime.UtcNow;
                    _context.PropostaHistoricos.Add(new PropostaHistoricoStatus
                    {
                        PropostaId = propostaVenda.Id,
                        StatusAnterior = anterior,
                        StatusNovo = StatusProposta.Aceita,
                        AlteradoPorUsuarioId = userId,
                        Motivo = $"Venda {venda.Numero} aprovada.",
                        DataAlteracao = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            _logger.LogInformation("Venda {VendaId} aprovada por {UserId}", venda.Id, userId);
            return NoContent();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    [HttpPost("{id:int}/contrato-assinado")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<IActionResult> ContratoAssinado(int id, [FromBody] ContratoAssinadoRequest request)
    {
        var venda = await _context.Vendas
            .Include(v => v.Comissoes)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (venda is null) return NotFound();

        if (venda.Status != StatusVenda.EmContrato)
            return Conflict(new { message = "Venda precisa estar em EmContrato." });

        venda.Status = StatusVenda.Assinada;
        venda.ContratoUrl = request.ContratoUrl;
        venda.UpdatedAt = DateTime.UtcNow;

        // Comissões pendentes viram Aprovadas (liberadas para pagamento)
        foreach (var comissao in venda.Comissoes.Where(c => c.Status == StatusComissao.Pendente))
        {
            comissao.Status = StatusComissao.Aprovada;
            comissao.DataAprovacao = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/cancelar")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<IActionResult> Cancelar(int id, [FromBody] CancelarVendaRequest request)
    {
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var venda = await _context.Vendas
                .Include(v => v.Apartamento)
                .Include(v => v.Comissoes)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (venda is null) return NotFound();
            if (venda.Status == StatusVenda.Cancelada) return NoContent();

            venda.Status = StatusVenda.Cancelada;
            venda.Observacoes = $"{venda.Observacoes}\n[CANCELADA] {request.Motivo}";
            venda.UpdatedAt = DateTime.UtcNow;

            if (venda.Apartamento.Status == StatusApartamento.Vendido)
                venda.Apartamento.Status = StatusApartamento.Disponivel;

            foreach (var comissao in venda.Comissoes.Where(c => c.Status != StatusComissao.Paga))
                comissao.Status = StatusComissao.Cancelada;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return NoContent();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    [HttpPut("comissoes/{comissaoId:int}/pagar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PagarComissao(int comissaoId)
    {
        var comissao = await _context.Comissoes.FirstOrDefaultAsync(c => c.Id == comissaoId);
        if (comissao is null) return NotFound();

        if (comissao.Status != StatusComissao.Aprovada)
            return Conflict(new { message = "Só é possível pagar comissões Aprovadas." });

        comissao.Status = StatusComissao.Paga;
        comissao.DataPagamento = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("comissoes/abertas")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<ActionResult<List<ComissaoResponse>>> ComissoesAbertas([FromQuery] int? corretorId)
    {
        var query = _context.Comissoes.AsNoTracking()
            .Include(c => c.Usuario)
            .Where(c => c.Status == StatusComissao.Aprovada || c.Status == StatusComissao.Pendente);

        if (corretorId.HasValue) query = query.Where(c => c.UsuarioId == corretorId);

        var items = await query.OrderBy(c => c.CreatedAt).ToListAsync();
        return Ok(_mapper.Map<List<ComissaoResponse>>(items));
    }

    private async Task<string> GerarNumeroAsync()
    {
        var ano = DateTime.UtcNow.Year;
        var count = await _context.Vendas.IgnoreQueryFilters()
            .CountAsync(v => v.CreatedAt.Year == ano);
        return $"VEND-{ano}-{(count + 1):D5}";
    }
}

public class ContratoAssinadoRequest
{
    public string ContratoUrl { get; set; } = string.Empty;
}

public class CancelarVendaRequest
{
    public string Motivo { get; set; } = string.Empty;
}
