using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ImovelStand.Application.Abstractions;
using ImovelStand.Application.Dtos;
using ImovelStand.Application.Services;

namespace ImovelStand.Api.Controllers;

/// <summary>
/// Endpoints transversais do módulo de IA:
/// - GET /consumo: resumo de uso do tenant atual (Admin/Gerente)
/// - POST /teste: invocação livre para QA/debug (Admin only)
///
/// Os endpoints de funcionalidades específicas (briefing de cliente, extrator
/// de proposta, etc.) ficam nos respectivos controllers de domínio.
/// </summary>
[Authorize]
[ApiController]
[Route("api/ia")]
public class IAController : ControllerBase
{
    private readonly IIAService _iaService;
    private readonly ITenantProvider _tenantProvider;

    public IAController(IIAService iaService, ITenantProvider tenantProvider)
    {
        _iaService = iaService;
        _tenantProvider = tenantProvider;
    }

    [HttpGet("consumo")]
    [Authorize(Roles = "Admin,Gerente")]
    public async Task<ActionResult<IAConsumoResumo>> Consumo(CancellationToken ct)
    {
        if (!_tenantProvider.HasTenant) return Unauthorized();
        var resumo = await _iaService.ObterConsumoAsync(_tenantProvider.TenantId, ct);
        return Ok(resumo);
    }

    [HttpPost("teste")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IAResponse>> Teste([FromBody] IARequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Operacao)) request.Operacao = "teste-admin";
        var response = await _iaService.InvocarAsync(request, ct);
        return Ok(response);
    }
}
