using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Domain.ValueObjects;

namespace ImovelStand.Tests.Services;

public class DashboardServiceTests
{
    private readonly DashboardService _sut = new();

    [Fact]
    public void Overview_CalculaKpisCorretamente()
    {
        var emp = new Empreendimento { Id = 1, Nome = "E" };
        var tipologia = new Tipologia { Id = 1, AreaPrivativa = 50, AreaTotal = 60, Quartos = 2, Banheiros = 1 };
        var apts = new List<Apartamento>
        {
            new() { Id = 1, Status = StatusApartamento.Vendido, PrecoAtual = 400_000m },
            new() { Id = 2, Status = StatusApartamento.Disponivel, PrecoAtual = 350_000m },
            new() { Id = 3, Status = StatusApartamento.Reservado, PrecoAtual = 360_000m },
            new() { Id = 4, Status = StatusApartamento.Proposta, PrecoAtual = 340_000m }
        };
        var now = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var vendas = new List<Venda>
        {
            new() { Status = StatusVenda.Assinada, DataFechamento = now.AddDays(-5), ValorFinal = 400_000m, CondicaoFinal = new CondicaoPagamento() },
            new() { Status = StatusVenda.EmContrato, DataFechamento = now.AddDays(-20), ValorFinal = 410_000m, CondicaoFinal = new CondicaoPagamento() }
        };

        var r = _sut.Overview(emp, apts, new[] { tipologia }, vendas, now);

        Assert.Equal(4, r.UnidadesTotal);
        Assert.Equal(1, r.UnidadesVendidas);
        Assert.Equal(1, r.UnidadesDisponiveis);
        Assert.Equal(1_450_000m, r.VgvTotal);
        Assert.Equal(400_000m, r.VgvVendido);
        Assert.Equal(0.25m, r.PctVendido);
        Assert.Equal(2, r.VendasUltimos30Dias);
        Assert.Equal(0.5m, r.VelocidadeVendaSemanal); // 2/4
    }

    [Fact]
    public void Funil_CalculaConversoesCorretamente()
    {
        var clientes = Enumerable.Range(1, 100).Select(i => new Cliente { Id = i, Nome = $"C{i}" }).ToList();
        var visitas = Enumerable.Range(1, 40).Select(i => new Visita { ClienteId = i, CorretorId = 1, EmpreendimentoId = 1 }).ToList();
        var propostas = Enumerable.Range(1, 20).Select(i => new Proposta { ClienteId = i, ApartamentoId = 1, CorretorId = 1, Numero = $"P{i}" }).ToList();
        var vendas = Enumerable.Range(1, 10).Select(i => new Venda
        {
            ClienteId = i, ApartamentoId = 1, CorretorId = 1, Numero = $"V{i}",
            Status = StatusVenda.Assinada,
            CondicaoFinal = new CondicaoPagamento()
        }).ToList();

        var r = _sut.Funil(clientes, visitas, propostas, vendas);

        Assert.Equal(100, r.Leads);
        Assert.Equal(40, r.Visitas);
        Assert.Equal(20, r.Propostas);
        Assert.Equal(10, r.Vendas);
        Assert.Equal(0.4m, r.ConversaoLeadParaVisita);
        Assert.Equal(0.5m, r.ConversaoVisitaParaProposta);
        Assert.Equal(0.5m, r.ConversaoPropostaParaVenda);
        Assert.Equal(0.1m, r.ConversaoGlobal);
    }

    [Fact]
    public void Funil_SemDados_NaoDivideParaZero()
    {
        var r = _sut.Funil(new List<Cliente>(), new List<Visita>(), new List<Proposta>(), new List<Venda>());
        Assert.Equal(0, r.ConversaoLeadParaVisita);
        Assert.Equal(0, r.ConversaoGlobal);
    }

    [Fact]
    public void Ranking_OrdenaPorVgvDescendente()
    {
        var a = new Usuario { Id = 1, Nome = "A" };
        var b = new Usuario { Id = 2, Nome = "B" };
        var c = new Usuario { Id = 3, Nome = "C" };
        var vendas = new List<Venda>
        {
            new() { CorretorId = 1, Status = StatusVenda.Assinada, ValorFinal = 400_000, CondicaoFinal = new CondicaoPagamento() },
            new() { CorretorId = 2, Status = StatusVenda.Assinada, ValorFinal = 600_000, CondicaoFinal = new CondicaoPagamento() },
            new() { CorretorId = 2, Status = StatusVenda.Assinada, ValorFinal = 500_000, CondicaoFinal = new CondicaoPagamento() },
            new() { CorretorId = 3, Status = StatusVenda.Cancelada, ValorFinal = 900_000, CondicaoFinal = new CondicaoPagamento() }
        };
        var ranking = _sut.Ranking(new[] { a, b, c }, vendas, new List<Comissao>(), new List<Visita>());

        Assert.Equal(3, ranking.Count);
        Assert.Equal(2, ranking[0].CorretorId); // B vendeu 1.1M
        Assert.Equal(1, ranking[1].CorretorId); // A vendeu 400k
        Assert.Equal(0, ranking[2].VgvVendido); // C cancelou - zero
    }
}
