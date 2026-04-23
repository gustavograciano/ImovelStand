using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ImovelStand.Application.Services;
using ImovelStand.Infrastructure.IA;

namespace ImovelStand.Api.Controllers;

/// <summary>
/// Endpoints do Copiloto IA para o corretor.
///
/// Convenção: prefixo /api/copiloto agrupa features de assistente IA
/// (briefing, próximas ações). Endpoints específicos de outras features
/// (extrator de proposta, análise de objeções) ficam em seus próprios
/// caminhos de domínio.
/// </summary>
[Authorize]
[ApiController]
[Route("api/copiloto")]
public class CopilotoController : ControllerBase
{
    private readonly CopilotoService _copiloto;

    public CopilotoController(CopilotoService copiloto)
    {
        _copiloto = copiloto;
    }

    /// <summary>
    /// Briefing de 3-5 linhas sobre um cliente para preparar o corretor
    /// antes da próxima interação.
    /// </summary>
    [HttpGet("briefing/{clienteId:int}")]
    public async Task<ActionResult<BriefingResponse>> Briefing(int clienteId, CancellationToken ct)
    {
        var resp = await _copiloto.GerarBriefingAsync(clienteId, ct);
        if (!resp.Sucesso && resp.MensagemErro?.Contains("não encontrado") == true)
            return NotFound(resp);
        return Ok(resp);
    }

    /// <summary>
    /// Fila priorizada de ações sugeridas para HOJE.
    /// Se admin/gerente, pode ver de qualquer corretor via querystring.
    /// Corretor só pode ver o próprio.
    /// </summary>
    [HttpGet("proximas-acoes")]
    public async Task<ActionResult<ProximasAcoesResponse>> ProximasAcoes(
        [FromQuery] int? corretorId,
        CancellationToken ct)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var userIdRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdRaw, out var userId)) return Unauthorized();

        var targetId = corretorId ?? userId;
        var isGestor = role == "Admin" || role == "Gerente";
        if (targetId != userId && !isGestor)
        {
            return Forbid();
        }

        var resp = await _copiloto.GerarProximasAcoesAsync(targetId, ct);
        return Ok(resp);
    }
}
