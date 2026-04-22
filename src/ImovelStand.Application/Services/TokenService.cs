using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ImovelStand.Domain.Entities;

namespace ImovelStand.Application.Services;

public class TokenService
{
    public const int RefreshTokenBytes = 48;
    public const int DefaultRefreshDays = 14;

    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string token, DateTime expiresAt) GenerateAccessToken(Usuario usuario)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
        var expirationHours = int.Parse(jwtSettings["ExpirationInHours"]!);
        var expiresAt = DateTime.UtcNow.AddHours(expirationHours);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Role),
                new Claim("tenantId", usuario.TenantId.ToString())
            }),
            Expires = expiresAt,
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(secretKey),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiresAt);
    }

    /// <summary>
    /// Gera refresh token opaco (base64) e o hash SHA-256 (para armazenar no DB).
    /// Nunca armazenamos o token em claro — padrão OWASP.
    /// </summary>
    public (string rawToken, string tokenHash, DateTime expiresAt) GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(RefreshTokenBytes);
        var raw = Convert.ToBase64String(bytes);
        var hash = HashRefreshToken(raw);
        var expiresAt = DateTime.UtcNow.AddDays(DefaultRefreshDays);
        return (raw, hash, expiresAt);
    }

    public static string HashRefreshToken(string rawToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashBytes);
    }

    [Obsolete("Mantido para compatibilidade; use GenerateAccessToken.")]
    public string GenerateToken(Usuario usuario) => GenerateAccessToken(usuario).token;

    public DateTime GetTokenExpiration()
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var expirationHours = int.Parse(jwtSettings["ExpirationInHours"]!);
        return DateTime.UtcNow.AddHours(expirationHours);
    }
}
