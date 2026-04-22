using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Dtos;
using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly TokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ApplicationDbContext context,
        TokenService tokenService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<TokenPairResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // IgnoreQueryFilters: login é cross-tenant — ninguém autenticado ainda.
            var usuario = await _context.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Ativo);

            if (usuario == null || !PasswordPolicy.Verify(request.Senha, usuario.SenhaHash))
            {
                return Unauthorized(new { message = "Email ou senha inválidos" });
            }

            var response = await EmitirTokensAsync(usuario);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar login");
            return StatusCode(500, new { message = "Erro interno ao processar login" });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenPairResponse>> Refresh([FromBody] RefreshRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "RefreshToken obrigatório." });
        }

        var hash = TokenService.HashRefreshToken(request.RefreshToken);

        // Cross-tenant: quem apresenta o refresh ainda não está autenticado.
        var token = await _context.RefreshTokens
            .IgnoreQueryFilters()
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (token is null || !token.EstaAtivo || !token.Usuario.Ativo)
        {
            return Unauthorized(new { message = "Refresh token inválido ou expirado." });
        }

        // Rotação: invalida o antigo e emite novo par.
        token.RevogadoEm = DateTime.UtcNow;
        token.MotivoRevogacao = "refresh_rotation";

        var response = await EmitirTokensAsync(token.Usuario, substituindoTokenId: token.Id);
        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return NoContent();
        }

        var hash = TokenService.HashRefreshToken(request.RefreshToken);
        var token = await _context.RefreshTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (token is { RevogadoEm: null })
        {
            token.RevogadoEm = DateTime.UtcNow;
            token.MotivoRevogacao = "logout";
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    private async Task<TokenPairResponse> EmitirTokensAsync(Usuario usuario, int? substituindoTokenId = null)
    {
        var (accessToken, accessExpira) = _tokenService.GenerateAccessToken(usuario);
        var (rawRefresh, refreshHash, refreshExpira) = _tokenService.GenerateRefreshToken();

        var refresh = new RefreshToken
        {
            TenantId = usuario.TenantId,
            UsuarioId = usuario.Id,
            TokenHash = refreshHash,
            ExpiraEm = refreshExpira,
            IpCriacao = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgentCriacao = Request.Headers.UserAgent.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refresh);
        await _context.SaveChangesAsync();

        if (substituindoTokenId is { } oldId)
        {
            var old = await _context.RefreshTokens
                .IgnoreQueryFilters()
                .FirstAsync(t => t.Id == oldId);
            old.SubstituidoPorTokenId = refresh.Id;
            await _context.SaveChangesAsync();
        }

        return new TokenPairResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiraEm = accessExpira,
            RefreshToken = rawRefresh,
            RefreshTokenExpiraEm = refreshExpira,
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Role = usuario.Role,
            TenantId = usuario.TenantId
        };
    }
}
