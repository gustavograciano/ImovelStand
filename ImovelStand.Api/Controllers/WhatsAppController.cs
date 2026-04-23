using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;
using ImovelStand.Infrastructure.WhatsApp;

namespace ImovelStand.Api.Controllers;

/// <summary>
/// Endpoints WhatsApp oficial:
/// - CRUD de templates (Admin/Gerente)
/// - Envio de template para cliente (qualquer usuário autenticado)
/// - Envio de texto livre (apenas dentro de janela 24h)
/// - Listagem de mensagens por cliente
/// </summary>
[Authorize]
[ApiController]
[Route("api/whatsapp")]
public class WhatsAppController : ControllerBase
{
    private readonly WhatsAppOficialService _service;
    private readonly ApplicationDbContext _context;

    public WhatsAppController(WhatsAppOficialService service, ApplicationDbContext context)
    {
        _service = service;
        _context = context;
    }

    // ========== Templates ==========

    [HttpGet("templates")]
    public async Task<ActionResult<List<WhatsAppTemplateDto>>> ListarTemplates(CancellationToken ct)
    {
        var items = await _context.WhatsAppTemplates.AsNoTracking()
            .OrderBy(t => t.Nome)
            .Select(t => new WhatsAppTemplateDto
            {
                Id = t.Id,
                Nome = t.Nome,
                Idioma = t.Idioma,
                Categoria = t.Categoria,
                Corpo = t.Corpo,
                QtdVariaveis = t.QtdVariaveis,
                Descricao = t.Descricao,
                Ativo = t.Ativo
            })
            .ToListAsync(ct);
        return Ok(items);
    }

    [HttpPost("templates")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<ActionResult<WhatsAppTemplateDto>> CriarTemplate([FromBody] WhatsAppTemplateCreateRequest req, CancellationToken ct)
    {
        if (await _context.WhatsAppTemplates.AnyAsync(t => t.Nome == req.Nome && t.Idioma == req.Idioma, ct))
            return Conflict(new { message = "Template com esse nome e idioma já existe." });

        var t = new WhatsAppTemplate
        {
            Nome = req.Nome,
            Idioma = req.Idioma,
            Categoria = req.Categoria,
            Corpo = req.Corpo,
            QtdVariaveis = req.QtdVariaveis,
            Descricao = req.Descricao,
            Ativo = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.WhatsAppTemplates.Add(t);
        await _context.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(ListarTemplates), new { id = t.Id }, new WhatsAppTemplateDto
        {
            Id = t.Id,
            Nome = t.Nome,
            Idioma = t.Idioma,
            Categoria = t.Categoria,
            Corpo = t.Corpo,
            QtdVariaveis = t.QtdVariaveis,
            Descricao = t.Descricao,
            Ativo = t.Ativo
        });
    }

    [HttpPut("templates/{id:int}")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<IActionResult> AtualizarTemplate(int id, [FromBody] WhatsAppTemplateUpdateRequest req, CancellationToken ct)
    {
        var t = await _context.WhatsAppTemplates.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (t is null) return NotFound();
        t.Corpo = req.Corpo;
        t.QtdVariaveis = req.QtdVariaveis;
        t.Descricao = req.Descricao;
        t.Ativo = req.Ativo;
        t.Categoria = req.Categoria;
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    // ========== Envio ==========

    [HttpPost("clientes/{clienteId:int}/enviar-template")]
    public async Task<ActionResult<WhatsAppMensagemDto>> EnviarTemplate(
        int clienteId,
        [FromBody] EnviarTemplateRequest req,
        CancellationToken ct)
    {
        var userIdRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? userId = int.TryParse(userIdRaw, out var u) ? u : null;

        try
        {
            var msg = await _service.EnviarTemplateParaClienteAsync(
                clienteId, req.TemplateId, req.Variaveis ?? new(), userId, ct);
            return Ok(Map(msg));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("clientes/{clienteId:int}/enviar-texto")]
    public async Task<ActionResult<WhatsAppMensagemDto>> EnviarTexto(
        int clienteId,
        [FromBody] EnviarTextoRequest req,
        CancellationToken ct)
    {
        var userIdRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? userId = int.TryParse(userIdRaw, out var u) ? u : null;

        try
        {
            var msg = await _service.EnviarTextoLivreAsync(clienteId, req.Texto, userId, ct);
            return Ok(Map(msg));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("clientes/{clienteId:int}/mensagens")]
    public async Task<ActionResult<List<WhatsAppMensagemDto>>> ListarMensagens(int clienteId, CancellationToken ct)
    {
        var msgs = await _service.ListarMensagensAsync(clienteId, ct);
        return Ok(msgs.Select(Map).ToList());
    }

    private static WhatsAppMensagemDto Map(WhatsAppMensagem m) => new()
    {
        Id = m.Id,
        ClienteId = m.ClienteId,
        TelefoneContato = m.TelefoneContato,
        Direcao = m.Direcao.ToString(),
        Status = m.Status.ToString(),
        Conteudo = m.Conteudo,
        TemplateId = m.TemplateId,
        MensagemErro = m.MensagemErro,
        CreatedAt = m.CreatedAt,
        EnviadaEm = m.EnviadaEm,
        EntregueEm = m.EntregueEm,
        LidaEm = m.LidaEm
    };
}

public class WhatsAppTemplateDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Idioma { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Corpo { get; set; } = string.Empty;
    public int QtdVariaveis { get; set; }
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}

public class WhatsAppTemplateCreateRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Idioma { get; set; } = "pt_BR";
    public string Categoria { get; set; } = "UTILITY";
    public string Corpo { get; set; } = string.Empty;
    public int QtdVariaveis { get; set; }
    public string? Descricao { get; set; }
}

public class WhatsAppTemplateUpdateRequest
{
    public string Categoria { get; set; } = "UTILITY";
    public string Corpo { get; set; } = string.Empty;
    public int QtdVariaveis { get; set; }
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}

public class EnviarTemplateRequest
{
    public int TemplateId { get; set; }
    public List<string>? Variaveis { get; set; }
}

public class EnviarTextoRequest
{
    public string Texto { get; set; } = string.Empty;
}

public class WhatsAppMensagemDto
{
    public long Id { get; set; }
    public int? ClienteId { get; set; }
    public string TelefoneContato { get; set; } = string.Empty;
    public string Direcao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public int? TemplateId { get; set; }
    public string? MensagemErro { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EnviadaEm { get; set; }
    public DateTime? EntregueEm { get; set; }
    public DateTime? LidaEm { get; set; }
}
