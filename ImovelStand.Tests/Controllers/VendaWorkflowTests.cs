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
using ImovelStand.Domain.ValueObjects;
using ImovelStand.Infrastructure.Persistence;
using ImovelStand.Tests.Fakes;

namespace ImovelStand.Tests.Controllers;

public class VendaWorkflowTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public VendaWorkflowTests()
    {
        _context = TestDbContextFactory.Create(new TestTenantProvider(Guid.Empty));
        var config = new TypeAdapterConfig();
        MappingRegistry.Register(config);
        _mapper = new Mapper(config);
    }

    private VendasController CreateController(int userId = 99)
    {
        var controller = new VendasController(_context, _mapper, new Mock<ILogger<VendasController>>().Object);
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "Test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        return controller;
    }

    private async Task<(int vendaId, int aptId)> SeedVendaNegociadaAsync(decimal percentualCorretor = 0.03m)
    {
        var corretor = new Usuario
        {
            TenantId = Guid.NewGuid(),
            Nome = "Corretor",
            Email = "c@c.com",
            SenhaHash = "x",
            Role = "Corretor",
            PercentualComissao = percentualCorretor,
            Ativo = true
        };
        var cliente = new Cliente
        {
            TenantId = Guid.NewGuid(),
            Nome = "Cliente",
            Cpf = "52998224725",
            Email = "cli@c.com",
            Telefone = "11"
        };
        var emp = new Empreendimento { TenantId = Guid.NewGuid(), Nome = "E", Slug = "e" };
        _context.Empreendimentos.Add(emp);
        await _context.SaveChangesAsync();
        var torre = new Torre { TenantId = emp.TenantId, EmpreendimentoId = emp.Id, Nome = "A", Pavimentos = 1, ApartamentosPorPavimento = 1 };
        var tipo = new Tipologia { TenantId = emp.TenantId, EmpreendimentoId = emp.Id, Nome = "T", AreaPrivativa = 50, AreaTotal = 60, Quartos = 2, Banheiros = 1, PrecoBase = 300_000 };
        _context.AddRange(torre, tipo, corretor, cliente);
        await _context.SaveChangesAsync();
        var apt = new Apartamento
        {
            TenantId = emp.TenantId,
            TorreId = torre.Id,
            TipologiaId = tipo.Id,
            Numero = "0101",
            Pavimento = 1,
            PrecoAtual = 300_000,
            Status = StatusApartamento.Disponivel
        };
        _context.Apartamentos.Add(apt);
        await _context.SaveChangesAsync();

        var venda = new Venda
        {
            TenantId = emp.TenantId,
            Numero = "VEND-2026-00001",
            ClienteId = cliente.Id,
            ApartamentoId = apt.Id,
            CorretorId = corretor.Id,
            ValorFinal = 300_000,
            Status = StatusVenda.Negociada,
            CondicaoFinal = new CondicaoPagamento { ValorTotal = 300_000, Entrada = 50_000, QtdParcelasMensais = 50, ValorParcelaMensal = 5000 },
            DataFechamento = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.Vendas.Add(venda);
        await _context.SaveChangesAsync();
        return (venda.Id, apt.Id);
    }

    [Fact]
    public async Task Aprovar_MudaAptoParaVendidoEVendaParaEmContrato()
    {
        var (vendaId, aptId) = await SeedVendaNegociadaAsync();

        var result = await CreateController().Aprovar(vendaId);
        Assert.IsType<NoContentResult>(result);

        var venda = await _context.Vendas.FirstAsync(v => v.Id == vendaId);
        var apt = await _context.Apartamentos.FirstAsync(a => a.Id == aptId);

        Assert.Equal(StatusVenda.EmContrato, venda.Status);
        Assert.NotNull(venda.DataAprovacao);
        Assert.Equal(99, venda.GerenteAprovadorId);
        Assert.Equal(StatusApartamento.Vendido, apt.Status);
    }

    [Fact]
    public async Task Aprovar_ComVendaNaoNegociada_RetornaConflict()
    {
        var (vendaId, _) = await SeedVendaNegociadaAsync();
        var venda = await _context.Vendas.FirstAsync(v => v.Id == vendaId);
        venda.Status = StatusVenda.Assinada;
        await _context.SaveChangesAsync();

        var result = await CreateController().Aprovar(vendaId);
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Aprovar_CancelaPropostasConcorrentesDoApto()
    {
        var (vendaId, aptId) = await SeedVendaNegociadaAsync();

        // Cria proposta concorrente Enviada no mesmo apto
        _context.Propostas.Add(new Proposta
        {
            TenantId = Guid.NewGuid(),
            Numero = "PROP-2026-00001",
            ClienteId = 1, ApartamentoId = aptId, CorretorId = 1,
            ValorOferecido = 300_000,
            Status = StatusProposta.Enviada,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        await CreateController().Aprovar(vendaId);

        var proposta = await _context.Propostas.FirstAsync(p => p.ApartamentoId == aptId);
        Assert.Equal(StatusProposta.Cancelada, proposta.Status);
    }

    [Fact]
    public async Task ContratoAssinado_AprovaComissoesPendentes()
    {
        var (vendaId, _) = await SeedVendaNegociadaAsync();
        await CreateController().Aprovar(vendaId);

        // Cria comissão manual pendente (o seed não gerou porque não passou pelo POST /vendas)
        _context.Comissoes.Add(new Comissao
        {
            TenantId = Guid.NewGuid(),
            VendaId = vendaId,
            UsuarioId = 1,
            Tipo = TipoComissao.Venda,
            Percentual = 0.03m,
            Valor = 9000m,
            Status = StatusComissao.Pendente
        });
        await _context.SaveChangesAsync();

        var result = await CreateController().ContratoAssinado(vendaId, new ContratoAssinadoRequest { ContratoUrl = "contrato.pdf" });
        Assert.IsType<NoContentResult>(result);

        var venda = await _context.Vendas.Include(v => v.Comissoes).FirstAsync(v => v.Id == vendaId);
        Assert.Equal(StatusVenda.Assinada, venda.Status);
        Assert.Equal("contrato.pdf", venda.ContratoUrl);
        Assert.All(venda.Comissoes, c => Assert.Equal(StatusComissao.Aprovada, c.Status));
    }

    [Fact]
    public async Task Cancelar_LiberaAptoEComissoesPendentes()
    {
        var (vendaId, aptId) = await SeedVendaNegociadaAsync();
        await CreateController().Aprovar(vendaId);

        _context.Comissoes.Add(new Comissao
        {
            TenantId = Guid.NewGuid(), VendaId = vendaId, UsuarioId = 1,
            Tipo = TipoComissao.Venda, Percentual = 0.03m, Valor = 9000m,
            Status = StatusComissao.Aprovada
        });
        await _context.SaveChangesAsync();

        var result = await CreateController().Cancelar(vendaId, new CancelarVendaRequest { Motivo = "Cliente desistiu" });
        Assert.IsType<NoContentResult>(result);

        var venda = await _context.Vendas.Include(v => v.Comissoes).FirstAsync(v => v.Id == vendaId);
        var apt = await _context.Apartamentos.FirstAsync(a => a.Id == aptId);
        Assert.Equal(StatusVenda.Cancelada, venda.Status);
        Assert.Equal(StatusApartamento.Disponivel, apt.Status);
        Assert.All(venda.Comissoes, c => Assert.Equal(StatusComissao.Cancelada, c.Status));
    }

    [Fact]
    public async Task PagarComissao_SoPagaSeAprovada()
    {
        var comissao = new Comissao
        {
            TenantId = Guid.NewGuid(), VendaId = 1, UsuarioId = 1,
            Tipo = TipoComissao.Venda, Percentual = 0.03m, Valor = 9000m,
            Status = StatusComissao.Pendente
        };
        _context.Comissoes.Add(comissao);
        await _context.SaveChangesAsync();

        var result = await CreateController().PagarComissao(comissao.Id);
        Assert.IsType<ConflictObjectResult>(result);

        comissao.Status = StatusComissao.Aprovada;
        await _context.SaveChangesAsync();

        result = await CreateController().PagarComissao(comissao.Id);
        Assert.IsType<NoContentResult>(result);

        var atualizada = await _context.Comissoes.FirstAsync(c => c.Id == comissao.Id);
        Assert.Equal(StatusComissao.Paga, atualizada.Status);
        Assert.NotNull(atualizada.DataPagamento);
    }
}
