using System.Threading.Tasks;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ImovelStand.Api.Controllers;
using ImovelStand.Application.Common;
using ImovelStand.Application.Dtos;
using ImovelStand.Application.Mapping;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;
using ImovelStand.Tests.Fakes;

namespace ImovelStand.Tests.Controllers;

public class ClientesControllerTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<ClientesController>> _loggerMock;

    public ClientesControllerTests()
    {
        _context = TestDbContextFactory.Create(new TestTenantProvider(Guid.Empty));
        _loggerMock = new Mock<ILogger<ClientesController>>();

        var config = new TypeAdapterConfig();
        MappingRegistry.Register(config);
        _mapper = new Mapper(config);
    }

    private ClientesController CreateController()
    {
        var controller = new ClientesController(_context, _mapper, _loggerMock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }

    [Fact]
    public async Task GetClientes_DeveRetornarPagedResult()
    {
        _context.Clientes.Add(new Cliente
        {
            Id = 1,
            TenantId = Guid.NewGuid(),
            Nome = "João Silva",
            Cpf = "12345678901",
            Email = "joao@test.com",
            Telefone = "11999999999"
        });
        await _context.SaveChangesAsync();

        var result = await CreateController().GetClientes(new PageRequest(), null, null, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var page = Assert.IsType<PagedResult<ClienteResponse>>(ok.Value);
        Assert.Single(page.Items);
        Assert.Equal(1, page.Total);
    }

    [Fact]
    public async Task GetCliente_ComIdValido_DeveRetornarCliente()
    {
        _context.Clientes.Add(new Cliente
        {
            Id = 2,
            TenantId = Guid.NewGuid(),
            Nome = "Maria Santos",
            Cpf = "98765432109",
            Email = "maria@test.com",
            Telefone = "11988888888"
        });
        await _context.SaveChangesAsync();

        var result = await CreateController().GetCliente(2);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ClienteResponse>(ok.Value);
        Assert.Equal("Maria Santos", dto.Nome);
    }

    [Fact]
    public async Task PostCliente_ComDadosValidos_DeveCriarCliente()
    {
        var request = new ClienteCreateRequest
        {
            Nome = "Pedro Oliveira",
            Cpf = "111.222.333-44",
            Email = "pedro@test.com",
            Telefone = "11977777777"
        };

        var result = await CreateController().PostCliente(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<ClienteResponse>(created.Value);
        Assert.Equal("Pedro Oliveira", dto.Nome);
        Assert.Equal("11122233344", dto.Cpf); // CPF normalizado
        Assert.True(dto.Id > 0);
    }

    [Fact]
    public async Task PostCliente_ComCpfDuplicado_DeveRetornarConflict()
    {
        _context.Clientes.Add(new Cliente
        {
            Id = 3,
            TenantId = Guid.NewGuid(),
            Nome = "Ana Costa",
            Cpf = "55566677788",
            Email = "ana@test.com",
            Telefone = "11966666666"
        });
        await _context.SaveChangesAsync();

        var request = new ClienteCreateRequest
        {
            Nome = "Outro Cliente",
            Cpf = "555.666.777-88",
            Email = "outro@test.com",
            Telefone = "11955555555"
        };

        var result = await CreateController().PostCliente(request);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }
}
