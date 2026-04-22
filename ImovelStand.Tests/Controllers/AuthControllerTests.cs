using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ImovelStand.Api.Controllers;
using ImovelStand.Infrastructure.Persistence;
using ImovelStand.Application.Dtos;
using ImovelStand.Domain.Entities;
using ImovelStand.Application.Services;
using ImovelStand.Tests.Fakes;
using Microsoft.Extensions.Configuration;

namespace ImovelStand.Tests.Controllers;

public class AuthControllerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly TokenService _tokenService;

    public AuthControllerTests()
    {
        _context = TestDbContextFactory.Create(new TestTenantProvider(Guid.Empty));
        _loggerMock = new Mock<ILogger<AuthController>>();

        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:SecretKey", "REPLACE_VIA_ENV_VAR_MIN_32_CHARS"},
            {"Jwt:Issuer", "ImovelStand.Api"},
            {"Jwt:Audience", "ImovelStand.Api"},
            {"Jwt:ExpirationInHours", "8"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _tokenService = new TokenService(configuration);

        _context.Usuarios.Add(new Usuario
        {
            Id = 1,
            TenantId = Guid.NewGuid(),
            Nome = "Teste Usuário",
            Email = "teste@test.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("Senha@123"),
            Role = "Admin",
            Ativo = true
        });
        _context.SaveChanges();
    }

    private AuthController CreateController()
    {
        var controller = new AuthController(_context, _tokenService, _loggerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarTokenPair()
    {
        var controller = CreateController();
        var request = new LoginRequest { Email = "teste@test.com", Senha = "Senha@123" };

        var result = await controller.Login(request);

        var actionResult = Assert.IsType<ActionResult<TokenPairResponse>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<TokenPairResponse>(okObjectResult.Value);
        Assert.False(string.IsNullOrEmpty(response.AccessToken));
        Assert.False(string.IsNullOrEmpty(response.RefreshToken));
        Assert.Equal("teste@test.com", response.Email);
        Assert.Equal("Admin", response.Role);
        Assert.NotEqual(Guid.Empty, response.TenantId);
    }

    [Fact]
    public async Task Login_ComEmailInvalido_DeveRetornarUnauthorized()
    {
        var controller = CreateController();
        var request = new LoginRequest { Email = "nao_existe@test.com", Senha = "Senha@123" };

        var result = await controller.Login(request);

        var actionResult = Assert.IsType<ActionResult<TokenPairResponse>>(result);
        Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task Login_ComSenhaInvalida_DeveRetornarUnauthorized()
    {
        var controller = CreateController();
        var request = new LoginRequest { Email = "teste@test.com", Senha = "SenhaErrada" };

        var result = await controller.Login(request);

        var actionResult = Assert.IsType<ActionResult<TokenPairResponse>>(result);
        Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task Refresh_ComTokenValido_RotacionaERetornaNovoPar()
    {
        var controller = CreateController();
        var loginRes = await controller.Login(new LoginRequest { Email = "teste@test.com", Senha = "Senha@123" });
        var original = (TokenPairResponse)((OkObjectResult)loginRes.Result!).Value!;

        var refreshRes = await controller.Refresh(new RefreshRequest { RefreshToken = original.RefreshToken });

        var actionResult = Assert.IsType<ActionResult<TokenPairResponse>>(refreshRes);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var renovado = Assert.IsType<TokenPairResponse>(ok.Value);
        Assert.NotEqual(original.RefreshToken, renovado.RefreshToken);

        // Token antigo deve estar revogado
        var hashAntigo = TokenService.HashRefreshToken(original.RefreshToken);
        var antigoNoDb = await _context.RefreshTokens.IgnoreQueryFilters().FirstAsync(t => t.TokenHash == hashAntigo);
        Assert.NotNull(antigoNoDb.RevogadoEm);
        Assert.Equal("refresh_rotation", antigoNoDb.MotivoRevogacao);
    }

    [Fact]
    public async Task Refresh_ComTokenInvalido_RetornaUnauthorized()
    {
        var controller = CreateController();

        var refreshRes = await controller.Refresh(new RefreshRequest { RefreshToken = "token-que-nao-existe" });

        var actionResult = Assert.IsType<ActionResult<TokenPairResponse>>(refreshRes);
        Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task Refresh_ComTokenJaRevogado_RetornaUnauthorized()
    {
        var controller = CreateController();
        var loginRes = await controller.Login(new LoginRequest { Email = "teste@test.com", Senha = "Senha@123" });
        var original = (TokenPairResponse)((OkObjectResult)loginRes.Result!).Value!;

        // Primeiro refresh consome o token
        await controller.Refresh(new RefreshRequest { RefreshToken = original.RefreshToken });

        // Segunda tentativa com o mesmo token deve falhar
        var segundo = await controller.Refresh(new RefreshRequest { RefreshToken = original.RefreshToken });

        Assert.IsType<UnauthorizedObjectResult>(segundo.Result);
    }

    [Fact]
    public async Task Logout_MarcaRefreshComoRevogado()
    {
        var controller = CreateController();
        var loginRes = await controller.Login(new LoginRequest { Email = "teste@test.com", Senha = "Senha@123" });
        var pair = (TokenPairResponse)((OkObjectResult)loginRes.Result!).Value!;

        var logoutRes = await controller.Logout(new LogoutRequest { RefreshToken = pair.RefreshToken });

        Assert.IsType<NoContentResult>(logoutRes);
        var hash = TokenService.HashRefreshToken(pair.RefreshToken);
        var tokenNoDb = await _context.RefreshTokens.IgnoreQueryFilters().FirstAsync(t => t.TokenHash == hash);
        Assert.NotNull(tokenNoDb.RevogadoEm);
        Assert.Equal("logout", tokenNoDb.MotivoRevogacao);
    }
}
