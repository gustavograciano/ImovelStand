using ImovelStand.Application.Abstractions;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;
using ImovelStand.Jobs.Jobs;
using ImovelStand.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ImovelStand.Tests.Jobs;

public class ExpirarJobsTests
{
    private readonly ApplicationDbContext _context;

    public ExpirarJobsTests()
    {
        _context = TestDbContextFactory.Create(new TestTenantProvider(Guid.Empty));
    }

    [Fact]
    public async Task ExpirarReservasJob_ExpirasReservasVencidasELiberaApto()
    {
        var tenant = Guid.NewGuid();
        var cliente = new Cliente { TenantId = tenant, Nome = "C", Cpf = "52998224725", Email = "c@c.com", Telefone = "1" };
        var emp = new Empreendimento { TenantId = tenant, Nome = "E", Slug = "e" };
        _context.AddRange(cliente, emp);
        await _context.SaveChangesAsync();
        var torre = new Torre { TenantId = tenant, EmpreendimentoId = emp.Id, Nome = "A", Pavimentos = 1, ApartamentosPorPavimento = 1 };
        var tipo = new Tipologia { TenantId = tenant, EmpreendimentoId = emp.Id, Nome = "T", AreaPrivativa = 50, AreaTotal = 60, Quartos = 2, Banheiros = 1, PrecoBase = 300_000 };
        _context.AddRange(torre, tipo);
        await _context.SaveChangesAsync();
        var apt = new Apartamento { TenantId = tenant, TorreId = torre.Id, TipologiaId = tipo.Id, Numero = "0101", Pavimento = 1, PrecoAtual = 300_000, Status = StatusApartamento.Reservado };
        _context.Apartamentos.Add(apt);
        await _context.SaveChangesAsync();

        _context.Reservas.Add(new Reserva
        {
            TenantId = tenant,
            ClienteId = cliente.Id,
            ApartamentoId = apt.Id,
            DataReserva = DateTime.UtcNow.AddDays(-10),
            DataExpiracao = DateTime.UtcNow.AddDays(-1),
            Status = "Ativa"
        });
        await _context.SaveChangesAsync();

        var notificador = new Mock<INotificador>();
        var job = new ExpirarReservasJob(_context, notificador.Object, NullLogger<ExpirarReservasJob>.Instance);

        await job.ExecuteAsync();

        var reserva = await _context.Reservas.FirstAsync();
        var aptAtual = await _context.Apartamentos.FirstAsync(a => a.Id == apt.Id);
        Assert.Equal("Expirada", reserva.Status);
        Assert.Equal(StatusApartamento.Disponivel, aptAtual.Status);
        notificador.Verify(n => n.EnviarEmailAsync(
            It.Is<string>(s => s == "c@c.com"), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExpirarPropostasJob_MarcaComoExpiradaERegistraHistorico()
    {
        var tenant = Guid.NewGuid();
        var cliente = new Cliente { TenantId = tenant, Nome = "C", Cpf = "52998224725", Email = "c@c.com", Telefone = "1" };
        var corretor = new Usuario { TenantId = tenant, Nome = "Cor", Email = "cor@c.com", SenhaHash = "x", Role = "Corretor", Ativo = true };
        _context.AddRange(cliente, corretor);
        await _context.SaveChangesAsync();

        _context.Propostas.Add(new Proposta
        {
            TenantId = tenant,
            Numero = "PROP-2026-00001",
            ClienteId = cliente.Id,
            ApartamentoId = 1,
            CorretorId = corretor.Id,
            ValorOferecido = 350_000,
            Status = StatusProposta.Enviada,
            DataEnvio = DateTime.UtcNow.AddDays(-10),
            DataValidade = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        });
        await _context.SaveChangesAsync();

        var job = new ExpirarPropostasJob(_context, NullLogger<ExpirarPropostasJob>.Instance);
        await job.ExecuteAsync();

        var proposta = await _context.Propostas.FirstAsync();
        var hist = await _context.PropostaHistoricos.FirstOrDefaultAsync();
        Assert.Equal(StatusProposta.Expirada, proposta.Status);
        Assert.NotNull(hist);
        Assert.Equal(StatusProposta.Expirada, hist!.StatusNovo);
    }
}
