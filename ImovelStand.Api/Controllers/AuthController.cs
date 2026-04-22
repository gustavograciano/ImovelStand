using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Infrastructure.Persistence;
using ImovelStand.Application.Dtos;
using ImovelStand.Application.Services;

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
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Ativo);

            if (usuario == null)
            {
                return Unauthorized(new { message = "Email ou senha inválidos" });
            }

            // Verificar senha com BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            {
                return Unauthorized(new { message = "Email ou senha inválidos" });
            }

            var token = _tokenService.GenerateToken(usuario);
            var expiration = _tokenService.GetTokenExpiration();

            var response = new LoginResponse
            {
                Token = token,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Role = usuario.Role,
                ExpiresAt = expiration
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar login");
            return StatusCode(500, new { message = "Erro interno ao processar login" });
        }
    }
}
