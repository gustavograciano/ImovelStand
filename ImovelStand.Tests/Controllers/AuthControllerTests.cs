using System.Threading.Tasks;
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

        // Configurar o TokenService
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

        // Seed de usuário para testes
        _context.Usuarios.Add(new Usuario
        {
            Id = 1,
            Nome = "Teste Usuário",
            Email = "teste@test.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("Senha@123"),
            Role = "Admin",
            Ativo = true
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarToken()
    {
        // Arrange
        var controller = new AuthController(_context, _tokenService, _loggerMock.Object);
        var loginRequest = new LoginRequest
        {
            Email = "teste@test.com",
            Senha = "Senha@123"
        };

        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<ActionResult<LoginResponse>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var loginResponse = Assert.IsType<LoginResponse>(okObjectResult.Value);
        Assert.NotNull(loginResponse.Token);
        Assert.Equal("teste@test.com", loginResponse.Email);
        Assert.Equal("Admin", loginResponse.Role);
    }

    [Fact]
    public async Task Login_ComEmailInvalido_DeveRetornarUnauthorized()
    {
        // Arrange
        var controller = new AuthController(_context, _tokenService, _loggerMock.Object);
        var loginRequest = new LoginRequest
        {
            Email = "email_invalido@test.com",
            Senha = "Senha@123"
        };

        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<ActionResult<LoginResponse>>(result);
        Assert.IsType<UnauthorizedObjectResult>(unauthorizedResult.Result);
    }

    [Fact]
    public async Task Login_ComSenhaInvalida_DeveRetornarUnauthorized()
    {
        // Arrange
        var controller = new AuthController(_context, _tokenService, _loggerMock.Object);
        var loginRequest = new LoginRequest
        {
            Email = "teste@test.com",
            Senha = "SenhaErrada"
        };

        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<ActionResult<LoginResponse>>(result);
        Assert.IsType<UnauthorizedObjectResult>(unauthorizedResult.Result);
    }
}
