using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WebhooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<WebhookSubscription>>> Listar() =>
        Ok(await _context.WebhookSubscriptions.AsNoTracking().OrderBy(w => w.Evento).ToListAsync());

    [HttpPost]
    public async Task<ActionResult<WebhookSubscription>> Criar([FromBody] WebhookSubscription sub)
    {
        sub.CreatedAt = DateTime.UtcNow;
        sub.FalhasConsecutivas = 0;
        _context.WebhookSubscriptions.Add(sub);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Listar), new { id = sub.Id }, sub);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] WebhookSubscription input)
    {
        var sub = await _context.WebhookSubscriptions.FirstOrDefaultAsync(w => w.Id == id);
        if (sub is null) return NotFound();
        sub.Url = input.Url;
        sub.Evento = input.Evento;
        sub.Secret = input.Secret;
        sub.Ativo = input.Ativo;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deletar(int id)
    {
        var sub = await _context.WebhookSubscriptions.FirstOrDefaultAsync(w => w.Id == id);
        if (sub is null) return NotFound();
        _context.WebhookSubscriptions.Remove(sub);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
