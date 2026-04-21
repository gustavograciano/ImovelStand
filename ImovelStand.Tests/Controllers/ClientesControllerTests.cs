using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ImovelStand.Api.Controllers;
using ImovelStand.Infrastructure.Persistence;
using ImovelStand.Domain.Entities;

namespace ImovelStand.Tests.Controllers;

public class ClientesControllerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<ClientesController>> _loggerMock;

    public ClientesControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_Clientes_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<ClientesController>>();
    }

    [Fact]
    public async Task GetClientes_DeveRetornarListaDeClientes()
    {
        // Arrange
        _context.Clientes.Add(new Cliente
        {
            Id = 1,
            Nome = "João Silva",
            Cpf = "12345678901",
            Email = "joao@test.com",
            Telefone = "11999999999"
        });
        await _context.SaveChangesAsync();

        var controller = new ClientesController(_context, _loggerMock.Object);

        // Act
        var result = await controller.GetClientes();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Cliente>>>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<Cliente>>(actionResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetCliente_ComIdValido_DeveRetornarCliente()
    {
        // Arrange
        _context.Clientes.Add(new Cliente
        {
            Id = 2,
            Nome = "Maria Santos",
            Cpf = "98765432109",
            Email = "maria@test.com",
            Telefone = "11988888888"
        });
        await _context.SaveChangesAsync();

        var controller = new ClientesController(_context, _loggerMock.Object);

        // Act
        var result = await controller.GetCliente(2);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Cliente>>(result);
        var returnValue = Assert.IsType<Cliente>(actionResult.Value);
        Assert.Equal("Maria Santos", returnValue.Nome);
    }

    [Fact]
    public async Task PostCliente_ComDadosValidos_DeveCriarCliente()
    {
        // Arrange
        var controller = new ClientesController(_context, _loggerMock.Object);
        var novoCliente = new Cliente
        {
            Nome = "Pedro Oliveira",
            Cpf = "11122233344",
            Email = "pedro@test.com",
            Telefone = "11977777777"
        };

        // Act
        var result = await controller.PostCliente(novoCliente);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Cliente>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var returnValue = Assert.IsType<Cliente>(createdAtActionResult.Value);
        Assert.Equal("Pedro Oliveira", returnValue.Nome);
        Assert.True(returnValue.Id > 0);
    }

    [Fact]
    public async Task PostCliente_ComCpfDuplicado_DeveRetornarBadRequest()
    {
        // Arrange
        _context.Clientes.Add(new Cliente
        {
            Id = 3,
            Nome = "Ana Costa",
            Cpf = "55566677788",
            Email = "ana@test.com",
            Telefone = "11966666666"
        });
        await _context.SaveChangesAsync();

        var controller = new ClientesController(_context, _loggerMock.Object);
        var novoCliente = new Cliente
        {
            Nome = "Outro Cliente",
            Cpf = "55566677788", // CPF duplicado
            Email = "outro@test.com",
            Telefone = "11955555555"
        };

        // Act
        var result = await controller.PostCliente(novoCliente);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Cliente>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }
}
