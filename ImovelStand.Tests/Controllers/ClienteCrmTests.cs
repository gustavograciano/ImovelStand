using System.Security.Claims;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ImovelStand.Api.Controllers;
using ImovelStand.Application.Dtos;
using ImovelStand.Application.Mapping;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;
using ImovelStand.Tests.Fakes;

namespace ImovelStand.Tests.Controllers;

public class ClienteCrmTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<ClientesController>> _loggerMock = new();

    public ClienteCrmTests()
    {
        _context = TestDbContextFactory.Create(new TestTenantProvider(Guid.Empty));

        var config = new TypeAdapterConfig();
        MappingRegistry.Register(config);
        _mapper = new Mapper(config);
    }

    private ClientesController CreateController(int? userId = 1)
    {
        var controller = new ClientesController(_context, _mapper, _loggerMock.Object);
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId?.ToString() ?? "")
        }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        return controller;
    }

    [Fact]
    public async Task PostCliente_ComConsentimentoLgpd_GravaTimestamp()
    {
        var request = new ClienteCreateRequest
        {
            Nome = "Fulano da Silva",
            Cpf = "529.982.247-25",
            Email = "fulano@test.com",
            Telefone = "11999999999",
            ConsentimentoLgpd = true
        };

        var result = await CreateController().PostCliente(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ClienteResponse>(created.Value);
        Assert.True(response.ConsentimentoLgpd);
        Assert.NotNull(response.ConsentimentoLgpdEm);
    }

    [Fact]
    public async Task PostCliente_CriaInteracaoInicialAutomatica()
    {
        var request = new ClienteCreateRequest
        {
            Nome = "Fulano da Silva",
            Cpf = "529.982.247-25",
            Email = "fulano@test.com",
            Telefone = "11999999999"
        };

        var result = await CreateController().PostCliente(request);
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<ClienteResponse>(created.Value);

        var interacoes = await _context.HistoricoInteracoes.Where(i => i.ClienteId == dto.Id).ToListAsync();
        Assert.Single(interacoes);
        Assert.Equal(TipoInteracao.MensagemInterna, interacoes[0].Tipo);
        Assert.Equal(1, interacoes[0].UsuarioId);
    }

    [Fact]
    public async Task PutCliente_MudandoStatusFunil_RegistraInteracao()
    {
        var id = await SeedClienteAsync();

        var request = new ClienteUpdateRequest
        {
            Nome = "Fulano da Silva",
            Email = "fulano@test.com",
            Telefone = "11999999999",
            StatusFunil = StatusFunil.Proposta
        };

        var result = await CreateController().PutCliente(id, request);
        Assert.IsType<NoContentResult>(result);

        var interacoes = await _context.HistoricoInteracoes.Where(i => i.ClienteId == id).ToListAsync();
        Assert.Contains(interacoes, i => i.Conteudo.Contains("Proposta"));
    }

    [Fact]
    public async Task ExportLgpd_RetornaClienteComInteracoes()
    {
        var id = await SeedClienteAsync(StatusFunil.Contato);
        _context.HistoricoInteracoes.Add(new HistoricoInteracao
        {
            ClienteId = id,
            Tipo = TipoInteracao.Whatsapp,
            Conteudo = "Cliente entrou em contato.",
            DataHora = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await CreateController().Export(id);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var export = Assert.IsType<ClienteLgpdExport>(ok.Value);

        Assert.Equal(id, export.Cliente.Id);
        Assert.NotEmpty(export.Interacoes);
    }

    [Fact]
    public async Task AtualizarConsentimentoLgpd_AceitoTrue_GravaTimestamp()
    {
        var id = await SeedClienteAsync();

        var result = await CreateController().AtualizarConsentimentoLgpd(id, new ConsentimentoLgpdRequest { Aceitou = true });
        Assert.IsType<NoContentResult>(result);

        var cliente = await _context.Clientes.FirstAsync(c => c.Id == id);
        Assert.True(cliente.ConsentimentoLgpd);
        Assert.NotNull(cliente.ConsentimentoLgpdEm);
    }

    private async Task<int> SeedClienteAsync(StatusFunil status = StatusFunil.Lead)
    {
        var cliente = new Cliente
        {
            TenantId = Guid.NewGuid(),
            Nome = "Fulano da Silva",
            Cpf = "52998224725",
            Email = "fulano@test.com",
            Telefone = "11999999999",
            StatusFunil = status,
            DataCadastro = DateTime.UtcNow
        };
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente.Id;
    }
}
