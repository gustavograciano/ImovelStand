using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Dtos;
using ImovelStand.Application.Services;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly DashboardService _dashboardService;
    private readonly ExcelExporter _excel;

    public DashboardController(ApplicationDbContext context, DashboardService dashboardService, ExcelExporter excel)
    {
        _context = context;
        _dashboardService = dashboardService;
        _excel = excel;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<DashboardOverviewResponse>> Overview([FromQuery] int empreendimentoId, CancellationToken ct)
    {
        var emp = await _context.Empreendimentos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == empreendimentoId, ct);
        if (emp is null) return NotFound();

        var torreIds = await _context.Torres.Where(t => t.EmpreendimentoId == empreendimentoId).Select(t => t.Id).ToListAsync(ct);
        var apts = await _context.Apartamentos.AsNoTracking().Where(a => torreIds.Contains(a.TorreId)).ToListAsync(ct);
        var tipologias = await _context.Tipologias.AsNoTracking().Where(t => t.EmpreendimentoId == empreendimentoId).ToListAsync(ct);
        var vendas = await _context.Vendas.AsNoTracking()
            .Where(v => apts.Select(a => a.Id).Contains(v.ApartamentoId))
            .ToListAsync(ct);

        return Ok(_dashboardService.Overview(emp, apts, tipologias, vendas));
    }

    [HttpGet("funil")]
    public async Task<ActionResult<FunilConversaoResponse>> Funil([FromQuery] int dias = 90, CancellationToken ct = default)
    {
        var desde = DateTime.UtcNow.AddDays(-dias);
        var clientes = await _context.Clientes.AsNoTracking().Where(c => c.DataCadastro >= desde).ToListAsync(ct);
        var visitas = await _context.Visitas.AsNoTracking().Where(v => v.DataHora >= desde).ToListAsync(ct);
        var propostas = await _context.Propostas.AsNoTracking().Where(p => p.CreatedAt >= desde).ToListAsync(ct);
        var vendas = await _context.Vendas.AsNoTracking().Where(v => v.DataFechamento >= desde).ToListAsync(ct);

        return Ok(_dashboardService.Funil(clientes, visitas, propostas, vendas, dias));
    }

    [HttpGet("ranking-corretores")]
    public async Task<ActionResult<List<RankingCorretorItem>>> RankingCorretores(CancellationToken ct)
    {
        var corretores = await _context.Usuarios.AsNoTracking()
            .Where(u => u.Role == "Corretor" || u.Role == "Gerente")
            .ToListAsync(ct);
        var vendas = await _context.Vendas.AsNoTracking().ToListAsync(ct);
        var comissoes = await _context.Comissoes.AsNoTracking().ToListAsync(ct);
        var visitas = await _context.Visitas.AsNoTracking().ToListAsync(ct);

        return Ok(_dashboardService.Ranking(corretores, vendas, comissoes, visitas));
    }

    [HttpGet("alertas")]
    public async Task<ActionResult<AlertasDashboard>> Alertas(CancellationToken ct)
    {
        var agora = DateTime.UtcNow;
        var em24h = agora.AddHours(24);

        var reservasExpirando = await _context.Reservas.AsNoTracking()
            .Include(r => r.Cliente)
            .Include(r => r.Apartamento)
            .Where(r => r.Status == "Ativa" && r.DataExpiracao != null && r.DataExpiracao <= em24h)
            .OrderBy(r => r.DataExpiracao)
            .Take(50)
            .Select(r => new ReservaExpirando(r.Id, r.ClienteId, r.Cliente.Nome, r.ApartamentoId, r.Apartamento.Numero, r.DataExpiracao!.Value))
            .ToListAsync(ct);

        var corte = agora.AddDays(-5);
        var propostasSemResposta = await _context.Propostas.AsNoTracking()
            .Include(p => p.Cliente)
            .Where(p => p.DataEnvio != null
                && p.DataEnvio <= corte
                && (p.Status == StatusProposta.Enviada
                    || p.Status == StatusProposta.ContrapropostaCorretor))
            .OrderBy(p => p.DataEnvio)
            .Take(50)
            .Select(p => new PropostaSemResposta(
                p.Id, p.Numero, p.ClienteId, p.Cliente.Nome, p.Status, p.DataEnvio!.Value,
                (int)(agora - p.DataEnvio!.Value).TotalDays))
            .ToListAsync(ct);

        return Ok(new AlertasDashboard
        {
            ReservasExpirando = reservasExpirando,
            PropostasSemResposta = propostasSemResposta
        });
    }

    [HttpGet("export/vendas.xlsx")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<IActionResult> ExportVendas([FromQuery] DateTime? desde, CancellationToken ct)
    {
        var query = _context.Vendas.AsNoTracking()
            .Include(v => v.Cliente)
            .Include(v => v.Apartamento).ThenInclude(a => a.Torre)
            .Include(v => v.Apartamento).ThenInclude(a => a.Tipologia)
            .Include(v => v.Corretor)
            .AsQueryable();
        if (desde.HasValue) query = query.Where(v => v.DataFechamento >= desde);
        var vendas = await query.ToListAsync(ct);
        var bytes = _excel.ExportarVendas(vendas);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"vendas-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("export/funil.xlsx")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<IActionResult> ExportFunil(CancellationToken ct)
    {
        var clientes = await _context.Clientes.AsNoTracking().ToListAsync(ct);
        var vendas = await _context.Vendas.AsNoTracking().ToListAsync(ct);
        var bytes = _excel.ExportarFunilPorOrigem(clientes, vendas);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"funil-origem-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}
