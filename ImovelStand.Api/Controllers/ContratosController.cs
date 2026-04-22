using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Abstractions;
using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/contratos")]
public class ContratosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorage _storage;
    private readonly ContratoTemplateEngine _engine;
    private readonly ILogger<ContratosController> _logger;

    public ContratosController(
        ApplicationDbContext context,
        IFileStorage storage,
        ContratoTemplateEngine engine,
        ILogger<ContratosController> logger)
    {
        _context = context;
        _storage = storage;
        _engine = engine;
        _logger = logger;
    }

    [HttpPost("templates")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ContratoTemplateResponse>> UploadTemplate(
        [FromForm] string nome,
        [FromForm] string? descricao,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Arquivo obrigatório." });
        if (!file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Apenas arquivos .docx são suportados." });

        var key = $"contratos/templates/{Guid.NewGuid():N}.docx";
        await using (var stream = file.OpenReadStream())
        {
            await _storage.UploadAsync(key, stream, file.ContentType, cancellationToken);
        }

        var template = new ContratoTemplate
        {
            Nome = nome,
            Descricao = descricao,
            ArquivoKey = key,
            CreatedAt = DateTime.UtcNow
        };
        _context.ContratoTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new ContratoTemplateResponse(template.Id, template.Nome, template.Descricao, template.Ativo));
    }

    [HttpGet("templates")]
    public async Task<ActionResult<List<ContratoTemplateResponse>>> ListarTemplates()
    {
        var templates = await _context.ContratoTemplates.AsNoTracking()
            .Where(t => t.Ativo)
            .OrderBy(t => t.Nome)
            .Select(t => new ContratoTemplateResponse(t.Id, t.Nome, t.Descricao, t.Ativo))
            .ToListAsync();
        return Ok(templates);
    }

    /// <summary>
    /// Gera DOCX substituindo placeholders do template com dados da venda.
    /// </summary>
    [HttpPost("vendas/{vendaId:int}/gerar")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<IActionResult> GerarContratoDeVenda(
        int vendaId,
        [FromQuery] int templateId,
        CancellationToken cancellationToken)
    {
        var template = await _context.ContratoTemplates.FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);
        if (template is null) return NotFound(new { message = "Template não encontrado." });

        var venda = await _context.Vendas.AsNoTracking()
            .Include(v => v.Cliente)
            .Include(v => v.Apartamento).ThenInclude(a => a.Torre)
            .Include(v => v.Apartamento).ThenInclude(a => a.Tipologia)
            .Include(v => v.Corretor)
            .FirstOrDefaultAsync(v => v.Id == vendaId, cancellationToken);
        if (venda is null) return NotFound(new { message = "Venda não encontrada." });

        var contexto = new Dictionary<string, object?>
        {
            ["venda"] = venda,
            ["cliente"] = venda.Cliente,
            ["apartamento"] = venda.Apartamento,
            ["torre"] = venda.Apartamento?.Torre,
            ["tipologia"] = venda.Apartamento?.Tipologia,
            ["corretor"] = venda.Corretor,
            ["condicao"] = venda.CondicaoFinal,
            ["hoje"] = DateTime.UtcNow
        };

        await using var templateStream = await _storage.DownloadAsync(template.ArquivoKey, cancellationToken);
        var bytes = _engine.Render(templateStream, contexto);

        _logger.LogInformation("Contrato gerado: venda={VendaId} template={TemplateId}", vendaId, templateId);

        var filename = $"contrato-{venda.Numero}-{DateTime.UtcNow:yyyyMMddHHmm}.docx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", filename);
    }

    [HttpPost("vendas/{vendaId:int}/assinado")]
    [Authorize(Roles = "Admin,Gerente")]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<IActionResult> UploadContratoAssinado(
        int vendaId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Arquivo obrigatório." });

        var venda = await _context.Vendas.Include(v => v.Comissoes).FirstOrDefaultAsync(v => v.Id == vendaId, cancellationToken);
        if (venda is null) return NotFound();

        if (venda.Status is not StatusVenda.EmContrato and not StatusVenda.Assinada)
            return Conflict(new { message = $"Venda precisa estar em EmContrato/Assinada (atual: {venda.Status})" });

        var key = $"contratos/assinados/{venda.Numero}-{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        await using (var stream = file.OpenReadStream())
        {
            await _storage.UploadAsync(key, stream, file.ContentType, cancellationToken);
        }

        venda.ContratoUrl = key;
        if (venda.Status == StatusVenda.EmContrato)
        {
            venda.Status = StatusVenda.Assinada;
            foreach (var c in venda.Comissoes.Where(c => c.Status == StatusComissao.Pendente))
            {
                c.Status = StatusComissao.Aprovada;
                c.DataAprovacao = DateTime.UtcNow;
            }
        }
        venda.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { contratoKey = key, status = venda.Status.ToString() });
    }
}

public record ContratoTemplateResponse(int Id, string Nome, string? Descricao, bool Ativo);
