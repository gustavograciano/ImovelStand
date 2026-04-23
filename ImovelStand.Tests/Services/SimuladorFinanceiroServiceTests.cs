using ImovelStand.Application.Services;

namespace ImovelStand.Tests.Services;

public class SimuladorFinanceiroServiceTests
{
    private readonly SimuladorFinanceiroService _sut = new();

    [Fact]
    public void CalcularCapacidade_Renda10k_Retorna3kComoLimite30Pct()
    {
        var r = _sut.CalcularCapacidade(rendaMensal: 10_000m);
        Assert.Equal(3_000m, r.ParcelaMaxima30Pct);
        Assert.Equal(3_500m, r.ParcelaMaxima35Pct);
        Assert.True(r.ImovelAproximadoSFH > 0);
    }

    [Fact]
    public void CalcularCapacidade_ComDividasRecorrentes_SubtraiDoLimite()
    {
        var r = _sut.CalcularCapacidade(rendaMensal: 10_000m, outrasDividas: 800m);
        Assert.Equal(2_200m, r.ParcelaMaxima30Pct);
    }

    [Fact]
    public void CalcularCapacidade_DividasAltas_GeraAlerta()
    {
        var r = _sut.CalcularCapacidade(rendaMensal: 10_000m, outrasDividas: 2_500m);
        Assert.NotNull(r.Alerta);
        Assert.Contains("20%", r.Alerta);
    }

    [Fact]
    public void CalcularSFH_PrimeiraParcelaMaiorQueUltimaEmSAC()
    {
        var r = _sut.CalcularSFH(valorImovel: 500_000m, entrada: 100_000m);
        // SAC: parcelas decrescentes
        Assert.True(r.PrimeiraParcela > r.UltimaParcela);
        Assert.Equal(360, r.PrazoMeses);
        Assert.Equal(400_000m, r.ValorFinanciado);
    }

    [Fact]
    public void CalcularSFH_JurosTotaisPositivos()
    {
        var r = _sut.CalcularSFH(valorImovel: 500_000m, entrada: 100_000m);
        Assert.True(r.JurosTotais > 0);
        Assert.True(r.CustoTotal > r.ValorFinanciado);
    }

    [Fact]
    public void CalcularSFH_EntradaMaiorQueValor_Lanca()
    {
        Assert.Throws<ArgumentException>(() =>
            _sut.CalcularSFH(valorImovel: 500_000m, entrada: 600_000m));
    }

    [Fact]
    public void CalcularSFI_MaisCaroQueSFH()
    {
        var sfh = _sut.CalcularSFH(500_000m, 100_000m);
        var sfi = _sut.CalcularSFI(500_000m, 100_000m);
        Assert.True(sfi.JurosTotais > sfh.JurosTotais);
    }

    [Fact]
    public void CalcularImpostos_SP_ItbiAoRedorDe3Pct()
    {
        var r = _sut.CalcularImpostos(500_000m, "SP");
        Assert.Equal(15_000m, r.Itbi); // 500k * 3%
        Assert.Equal("SP", r.Uf);
    }

    [Fact]
    public void CalcularImpostos_UfDesconhecida_UsaPadrao()
    {
        var r = _sut.CalcularImpostos(500_000m, "XX");
        Assert.True(r.Itbi > 0);
    }

    [Fact]
    public void CalcularParcelamentoDireto_SemReajuste_ParcelaInicialIgualFinal()
    {
        var r = _sut.CalcularParcelamentoDireto(600_000m, 60_000m, 120, taxaReajusteAnual: 0m);
        Assert.Equal(r.ParcelaInicial, r.ParcelaFinal);
        Assert.Equal(4_500m, r.ParcelaInicial); // (600k-60k)/120
    }

    [Fact]
    public void CalcularParcelamentoDireto_ComReajuste_ParcelaCresce()
    {
        var r = _sut.CalcularParcelamentoDireto(600_000m, 60_000m, 120, taxaReajusteAnual: 0.08m);
        Assert.True(r.ParcelaFinal > r.ParcelaInicial);
    }

    [Fact]
    public void CompararAluguelVsCompra_CenarioTipico_RetornaResultado()
    {
        var r = _sut.CompararAluguelVsCompra(
            valorImovel: 500_000m,
            entrada: 100_000m,
            aluguelMensalAtual: 2_500m);
        Assert.True(r.ValorImovelFinal > r.ValorImovelInicial);
        Assert.True(r.GastoTotalComprar > 0);
        Assert.True(r.GastoTotalAlugar > 0);
        Assert.NotEmpty(r.Recomendacao);
    }

    [Fact]
    public void SimulacaoCompleta_TodosOsCamposPreenchidos()
    {
        var r = _sut.SimulacaoCompleta(new SimulacaoCompletaRequest
        {
            ValorImovel = 500_000m,
            Entrada = 100_000m,
            RendaMensal = 15_000m,
            OutrasDividas = 0,
            AluguelAtual = 2_500m,
            QtdParcelasDireto = 120,
            Uf = "SP"
        });
        Assert.NotNull(r.Sfh);
        Assert.NotNull(r.Sfi);
        Assert.NotNull(r.Impostos);
        Assert.NotNull(r.Capacidade);
        Assert.NotNull(r.AluguelVsCompra);
        Assert.NotNull(r.ParcelamentoDireto);
        Assert.NotNull(r.ParcelaCabe);
        Assert.NotEmpty(r.ResumoExecutivo);
    }
}
