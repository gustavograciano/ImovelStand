using ImovelStand.Application.Services;
using ImovelStand.Domain.Enums;
using ImovelStand.Domain.ValueObjects;

namespace ImovelStand.Tests.Services;

public class CalculadoraFinanceiraTests
{
    private readonly CalculadoraFinanceira _sut = new();

    [Fact]
    public void Distribuir_CalculaValorParcelaMensalCorretamente()
    {
        var condicao = new CondicaoPagamento
        {
            ValorTotal = 500_000m,
            Entrada = 100_000m,
            Sinal = 20_000m,
            ValorChaves = 50_000m,
            QtdParcelasMensais = 60,
            Indice = IndiceReajuste.SemReajuste
        };

        _sut.Distribuir(condicao);

        // Restante = 500k - 100k - 20k - 50k = 330k / 60 = 5500
        Assert.Equal(5_500m, condicao.ValorParcelaMensal);
    }

    [Fact]
    public void Distribuir_ExcedeValorTotal_Lanca()
    {
        var condicao = new CondicaoPagamento
        {
            ValorTotal = 100_000m,
            Entrada = 150_000m, // maior que o total
            QtdParcelasMensais = 10
        };

        Assert.Throws<InvalidOperationException>(() => _sut.Distribuir(condicao));
    }

    [Fact]
    public void Distribuir_ValorTotalZero_Lanca()
    {
        Assert.Throws<ArgumentException>(() =>
            _sut.Distribuir(new CondicaoPagamento { ValorTotal = 0 }));
    }

    [Fact]
    public void ProjetarParcelas_GeraNaOrdemCronologica()
    {
        var condicao = new CondicaoPagamento
        {
            ValorTotal = 300_000m,
            Entrada = 50_000m,
            EntradaData = new DateTime(2026, 1, 1),
            QtdParcelasMensais = 6,
            ValorParcelaMensal = 40_000m,
            PrimeiraParcelaData = new DateTime(2026, 2, 1),
            Indice = IndiceReajuste.SemReajuste,
            ValorChaves = 10_000m,
            ChavesDataPrevista = new DateTime(2026, 12, 1)
        };

        var parcelas = _sut.ProjetarParcelas(condicao);

        Assert.True(parcelas.Count > 0);
        for (var i = 1; i < parcelas.Count; i++)
        {
            Assert.True(parcelas[i].Data >= parcelas[i - 1].Data);
        }
    }

    [Fact]
    public void ProjetarParcelas_ComReajusteINCC_AumentaParcelasFuturas()
    {
        var condicao = new CondicaoPagamento
        {
            ValorTotal = 360_000m,
            QtdParcelasMensais = 12,
            ValorParcelaMensal = 30_000m,
            PrimeiraParcelaData = new DateTime(2026, 1, 1),
            Indice = IndiceReajuste.Incc
        };

        var parcelas = _sut.ProjetarParcelas(condicao, taxaIndiceAnual: 0.12m);
        var mensais = parcelas.Where(p => p.Descricao.StartsWith("Parcela")).ToList();

        Assert.Equal(12, mensais.Count);
        Assert.True(mensais[^1].Valor > mensais[0].Valor, "Última parcela deve estar reajustada.");
    }
}
