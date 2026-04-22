using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IuguBillingService _billing;
    private readonly ILogger<BillingController> _logger;

    public BillingController(ApplicationDbContext context, IuguBillingService billing, ILogger<BillingController> logger)
    {
        _context = context;
        _billing = billing;
        _logger = logger;
    }

    [HttpGet("assinatura")]
    public async Task<ActionResult<Assinatura?>> Atual(CancellationToken ct)
    {
        var assinatura = await _context.Set<Assinatura>().AsNoTracking()
            .Include(a => a.Plano)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(ct);
        return Ok(assinatura);
    }

    [HttpPost("iniciar-trial")]
    [AllowAnonymous]
    public async Task<ActionResult<Assinatura>> IniciarTrial([FromBody] IniciarTrialRequest request, CancellationToken ct)
    {
        if (await _context.Tenants.AnyAsync(t => t.Slug == request.Slug, ct))
            return Conflict(new { message = "Slug já em uso." });

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Nome = request.NomeEmpresa,
            Cnpj = request.Cnpj,
            Slug = request.Slug,
            PlanoId = request.PlanoId,
            TrialAte = DateTime.UtcNow.AddDays(14),
            Ativo = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Tenants.Add(tenant);

        var plano = await _context.Planos.FirstAsync(p => p.Id == request.PlanoId, ct);
        var customerId = await _billing.CreateCustomerAsync(tenant, request.EmailAdmin, ct);
        var subscriptionId = await _billing.CreateSubscriptionAsync(customerId, plano, ct);

        var assinatura = new Assinatura
        {
            TenantId = tenant.Id,
            PlanoId = plano.Id,
            Status = StatusAssinatura.Trial,
            DataInicio = DateTime.UtcNow,
            TrialFimEm = DateTime.UtcNow.AddDays(14),
            IuguCustomerId = customerId,
            IuguSubscriptionId = subscriptionId,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<Assinatura>().Add(assinatura);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Trial iniciado: tenant {Slug} no plano {Plano}", tenant.Slug, plano.Nome);

        return Created($"/api/billing/assinatura", assinatura);
    }

    [HttpPost("webhook/iugu")]
    [AllowAnonymous]
    public async Task<IActionResult> IuguWebhook([FromBody] IuguWebhookPayload payload, CancellationToken ct)
    {
        _logger.LogInformation("Webhook Iugu recebido: evento={Evento} sub={Sub}", payload.Event, payload.Data.SubscriptionId);

        var assinatura = await _context.Set<Assinatura>()
            .FirstOrDefaultAsync(a => a.IuguSubscriptionId == payload.Data.SubscriptionId, ct);
        if (assinatura is null) return Ok();

        assinatura.Status = payload.Event switch
        {
            "invoice.status_changed" when payload.Data.Status == "paid" => StatusAssinatura.Ativa,
            "invoice.status_changed" when payload.Data.Status == "expired" => StatusAssinatura.Inadimplente,
            "subscription.activated" => StatusAssinatura.Ativa,
            "subscription.suspended" => StatusAssinatura.Suspensa,
            "subscription.canceled" => StatusAssinatura.Cancelada,
            _ => assinatura.Status
        };
        assinatura.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return Ok();
    }

    public record IniciarTrialRequest(string NomeEmpresa, string? Cnpj, string Slug, int PlanoId, string EmailAdmin);
    public record IuguWebhookPayload(string Event, IuguWebhookData Data);
    public record IuguWebhookData(string SubscriptionId, string? Status);
}
