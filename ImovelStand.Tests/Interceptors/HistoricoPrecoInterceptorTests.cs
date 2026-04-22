using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Interceptors;
using ImovelStand.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ImovelStand.Tests.Interceptors;

public class HistoricoPrecoInterceptorTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new HistoricoPrecoInterceptor())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<Apartamento> SeedApartamentoAsync(ApplicationDbContext ctx, decimal precoInicial)
    {
        var emp = new Empreendimento { Id = 1, Nome = "Emp", Slug = "emp" };
        var torre = new Torre { Id = 1, EmpreendimentoId = 1, Nome = "A", Pavimentos = 1, ApartamentosPorPavimento = 1 };
        var tipologia = new Tipologia { Id = 1, EmpreendimentoId = 1, Nome = "T", AreaPrivativa = 50, AreaTotal = 60, Quartos = 2, Banheiros = 1, PrecoBase = precoInicial };
        var apt = new Apartamento { Id = 1, TorreId = 1, TipologiaId = 1, Numero = "0101", Pavimento = 1, PrecoAtual = precoInicial, Status = StatusApartamento.Disponivel };

        ctx.AddRange(emp, torre, tipologia, apt);
        await ctx.SaveChangesAsync();
        return apt;
    }

    [Fact]
    public async Task AlterarPrecoAtual_DeveCriarHistoricoPreco()
    {
        using var ctx = CreateContext();
        var apt = await SeedApartamentoAsync(ctx, 300_000m);

        apt.PrecoAtual = 330_000m;
        await ctx.SaveChangesAsync();

        var historico = await ctx.HistoricoPrecos.SingleAsync();
        Assert.Equal(apt.Id, historico.ApartamentoId);
        Assert.Equal(300_000m, historico.PrecoAnterior);
        Assert.Equal(330_000m, historico.PrecoNovo);
    }

    [Fact]
    public async Task AtualizarApartamentoSemMudarPreco_NaoCriaHistoricoPreco()
    {
        using var ctx = CreateContext();
        var apt = await SeedApartamentoAsync(ctx, 300_000m);

        apt.Observacoes = "qualquer coisa";
        await ctx.SaveChangesAsync();

        Assert.Equal(0, await ctx.HistoricoPrecos.CountAsync());
    }

    [Fact]
    public async Task SetarMesmoPrecoAtual_NaoCriaHistoricoPreco()
    {
        using var ctx = CreateContext();
        var apt = await SeedApartamentoAsync(ctx, 300_000m);

        apt.PrecoAtual = 300_000m;
        await ctx.SaveChangesAsync();

        Assert.Equal(0, await ctx.HistoricoPrecos.CountAsync());
    }
}
