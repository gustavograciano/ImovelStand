using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Services;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/empreendimentos/{empreendimentoId:int}/espelho")]
public class EspelhoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly EspelhoPdfGenerator _generator;
    private readonly ILogger<EspelhoController> _logger;

    public EspelhoController(ApplicationDbContext context, EspelhoPdfGenerator generator, ILogger<EspelhoController> logger)
    {
        _context = context;
        _generator = generator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GerarEspelho(
        int empreendimentoId,
        [FromQuery] TipoEspelho tipo = TipoEspelho.Comercial,
        CancellationToken cancellationToken = default)
    {
        var empreendimento = await _context.Empreendimentos.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == empreendimentoId, cancellationToken);
        if (empreendimento is null) return NotFound();

        var torres = await _context.Torres.AsNoTracking()
            .Where(t => t.EmpreendimentoId == empreendimentoId)
            .OrderBy(t => t.Nome)
            .ToListAsync(cancellationToken);

        var tipologias = await _context.Tipologias.AsNoTracking()
            .Where(t => t.EmpreendimentoId == empreendimentoId)
            .ToListAsync(cancellationToken);

        var torreIds = torres.Select(t => t.Id).ToArray();
        var apartamentos = await _context.Apartamentos.AsNoTracking()
            .Where(a => torreIds.Contains(a.TorreId))
            .ToListAsync(cancellationToken);

        var metadata = new EspelhoMetadata(
            TenantNome: empreendimento.Nome,
            GeradoPor: User.Identity?.Name ?? User.FindFirst(ClaimTypes.Email)?.Value ?? "desconhecido",
            GeradoEm: DateTime.UtcNow);

        var pdf = _generator.Gerar(tipo, empreendimento, torres, tipologias, apartamentos, metadata);

        _logger.LogInformation("Espelho {Tipo} gerado para empreendimento {Id} por {User}",
            tipo, empreendimentoId, metadata.GeradoPor);

        var filename = $"espelho-{empreendimento.Slug}-{tipo.ToString().ToLowerInvariant()}-{DateTime.UtcNow:yyyyMMddHHmm}.pdf";
        return File(pdf, "application/pdf", filename);
    }
}
