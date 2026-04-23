namespace ImovelStand.Application.Services;

/// <summary>
/// Motor de simulação financeira para cliente final. Puro (sem DbContext) —
/// testável unitariamente e embeddable em widget standalone.
///
/// Cobre:
/// - Financiamento SFH (Caixa): SAC (Sistema de Amortização Constante)
/// - Financiamento SFI (Sistema Financeiro Imobiliário)
/// - Parcelamento direto com incorporadora (com reajuste)
/// - Impostos de compra (ITBI + cartório + registro) por UF
/// - Comparação aluguel vs compra ao longo de 30 anos
/// - Capacidade de pagamento (regra 30% da renda líquida)
///
/// Todas as simulações são APROXIMADAS. Cliente deve confirmar com banco.
/// </summary>
public class SimuladorFinanceiroService
{
    // ITBI por UF (tabela aproximada, média das capitais). Algumas cidades
    // cobram 2%, outras até 4%. Melhor que esconder, é mostrar valor médio.
    private static readonly Dictionary<string, decimal> ItbiPorUf = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SP"] = 0.030m,  // São Paulo: 3%
        ["RJ"] = 0.030m,  // Rio de Janeiro: 3%
        ["MG"] = 0.030m,  // Belo Horizonte: 3%
        ["PR"] = 0.027m,  // Curitiba: 2,7%
        ["RS"] = 0.030m,  // Porto Alegre: 3%
        ["BA"] = 0.030m,  // Salvador: 3%
        ["PE"] = 0.030m,
        ["CE"] = 0.030m,
        ["DF"] = 0.030m,
        ["SC"] = 0.020m,  // Florianópolis: 2%
        ["GO"] = 0.020m,
        ["ES"] = 0.020m,
        ["PA"] = 0.020m,
        ["AM"] = 0.020m,
        ["MT"] = 0.020m,
        ["MS"] = 0.020m,
        ["RN"] = 0.020m
    };

    private const decimal ITBI_PADRAO = 0.030m;

    // ========== Capacidade de pagamento ==========

    public CapacidadePagamentoResult CalcularCapacidade(decimal rendaMensal, decimal outrasDividas = 0)
    {
        // Regra 30% clássica: 30% da renda líquida pode ser comprometida com moradia
        var maxParcela = Math.Max(0, (rendaMensal * 0.30m) - outrasDividas);
        // Comprometimento máximo realista: 35% em casos específicos
        var maxParcelaAgressiva = Math.Max(0, (rendaMensal * 0.35m) - outrasDividas);

        return new CapacidadePagamentoResult
        {
            RendaMensal = rendaMensal,
            OutrasDividas = outrasDividas,
            ParcelaMaxima30Pct = Math.Round(maxParcela, 2),
            ParcelaMaxima35Pct = Math.Round(maxParcelaAgressiva, 2),
            ImovelAproximadoSFH = EstimarImovelPorParcelaSFH(maxParcela),
            Alerta = rendaMensal <= 0
                ? "Renda inválida."
                : outrasDividas > rendaMensal * 0.20m
                    ? "Atenção: suas dívidas existentes já comprometem >20% da renda. Capacidade real pode ser menor."
                    : null
        };
    }

    private decimal EstimarImovelPorParcelaSFH(decimal parcelaMax, decimal prazoAnos = 30)
    {
        // Aproximação SAC primeira parcela: considera que primeira parcela
        // ≈ (valor financiado / prazo_meses) + (valor financiado * taxa_mensal)
        // Taxa SFH Caixa aproximada 2026: 9,75% a.a. = 0,78% a.m.
        if (parcelaMax <= 0) return 0;
        var taxa = 0.0078m;
        var meses = prazoAnos * 12;
        // Equivalente: P = F * (1/meses + taxa)  →  F = P / (1/meses + taxa)
        var valorFinanciado = parcelaMax / (1m / meses + taxa);
        // Assume entrada de 20%
        var valorImovel = valorFinanciado / 0.80m;
        // Arredonda para o milhar mais próximo
        return Math.Round(valorImovel / 1000m, 0, MidpointRounding.AwayFromZero) * 1000m;
    }

    // ========== SFH (Caixa) - SAC ==========

    public FinanciamentoResult CalcularSFH(
        decimal valorImovel,
        decimal entrada,
        decimal prazoAnos = 30,
        decimal taxaAnual = 0.0975m)
    {
        if (valorImovel <= 0) throw new ArgumentException("Valor do imóvel inválido.");
        if (entrada < 0) throw new ArgumentException("Entrada inválida.");
        if (entrada >= valorImovel) throw new ArgumentException("Entrada >= valor do imóvel.");

        var valorFinanciado = valorImovel - entrada;
        var meses = (int)(prazoAnos * 12);
        var taxaMensal = (decimal)Math.Pow((double)(1 + taxaAnual), 1.0 / 12.0) - 1m;

        // SAC: amortização constante, juros decrescentes
        var amortizacao = valorFinanciado / meses;
        var saldoDevedor = valorFinanciado;
        decimal jurosTotais = 0;
        decimal primeiraParcela = 0;
        decimal ultimaParcela = 0;

        for (var m = 0; m < meses; m++)
        {
            var juros = saldoDevedor * taxaMensal;
            var parcela = amortizacao + juros;
            if (m == 0) primeiraParcela = parcela;
            if (m == meses - 1) ultimaParcela = parcela;
            jurosTotais += juros;
            saldoDevedor -= amortizacao;
        }

        return new FinanciamentoResult
        {
            Sistema = "SFH (Caixa - SAC)",
            ValorImovel = valorImovel,
            Entrada = entrada,
            ValorFinanciado = valorFinanciado,
            PctEntrada = Math.Round(entrada / valorImovel * 100m, 2),
            PrazoMeses = meses,
            TaxaAnualPct = Math.Round(taxaAnual * 100m, 2),
            PrimeiraParcela = Math.Round(primeiraParcela, 2),
            UltimaParcela = Math.Round(ultimaParcela, 2),
            JurosTotais = Math.Round(jurosTotais, 2),
            CustoTotal = Math.Round(valorFinanciado + jurosTotais, 2),
            Cet = Math.Round(taxaAnual * 100m + 0.5m, 2) // CET aproximado = taxa + seguros
        };
    }

    // ========== SFI ==========

    public FinanciamentoResult CalcularSFI(
        decimal valorImovel,
        decimal entrada,
        decimal prazoAnos = 30,
        decimal taxaAnual = 0.1100m) // SFI é mais caro
    {
        var r = CalcularSFH(valorImovel, entrada, prazoAnos, taxaAnual);
        r.Sistema = "SFI (SAC)";
        return r;
    }

    // ========== Impostos de compra ==========

    public ImpostosCompraResult CalcularImpostos(decimal valorImovel, string uf)
    {
        var itbiTaxa = ItbiPorUf.TryGetValue(uf ?? string.Empty, out var t) ? t : ITBI_PADRAO;
        var itbi = valorImovel * itbiTaxa;
        // Cartório + registro: aproximação tabelada ~1% sobre valor do imóvel
        var cartorio = valorImovel * 0.010m;
        var total = itbi + cartorio;

        return new ImpostosCompraResult
        {
            Uf = (uf ?? "SP").ToUpperInvariant(),
            ValorImovel = valorImovel,
            ItbiPct = Math.Round(itbiTaxa * 100m, 2),
            Itbi = Math.Round(itbi, 2),
            Cartorio = Math.Round(cartorio, 2),
            Total = Math.Round(total, 2),
            PctSobreImovel = Math.Round(total / valorImovel * 100m, 2)
        };
    }

    // ========== Aluguel vs Compra (30 anos) ==========

    public AluguelVsCompraResult CompararAluguelVsCompra(
        decimal valorImovel,
        decimal entrada,
        decimal aluguelMensalAtual,
        decimal selicAnual = 0.1050m,
        decimal ipcaAnual = 0.045m,
        decimal valorizacaoImovelAnual = 0.06m,
        decimal prazoAnos = 30,
        decimal taxaSfhAnual = 0.0975m)
    {
        var sfh = CalcularSFH(valorImovel, entrada, prazoAnos, taxaSfhAnual);

        // Cenário COMPRAR:
        // - Paga entrada + parcelas SFH
        // - Imóvel valoriza anualmente
        // - Patrimônio final = valor do imóvel (valorizado) - saldo devedor (já quitado em 30a)
        var valorImovelFinal = valorImovel * (decimal)Math.Pow((double)(1 + valorizacaoImovelAnual), (double)prazoAnos);
        var gastoTotalCompra = entrada + sfh.JurosTotais + sfh.ValorFinanciado;

        // Cenário ALUGAR + INVESTIR:
        // - Paga aluguel ajustado por IPCA
        // - Diferença (parcela SFH - aluguel) é investida a Selic
        // - Entrada é investida a Selic desde o início
        decimal patrimonioInvestimento = entrada;
        decimal gastoTotalAluguel = 0;
        var taxaSelicMensal = (decimal)Math.Pow((double)(1 + selicAnual), 1.0 / 12.0) - 1m;
        var taxaIpcaMensal = (decimal)Math.Pow((double)(1 + ipcaAnual), 1.0 / 12.0) - 1m;
        var aluguelMes = aluguelMensalAtual;
        // parcela SAC varia; pra simplificar, uso parcela média entre primeira e última
        var parcelaMedia = (sfh.PrimeiraParcela + sfh.UltimaParcela) / 2m;
        var meses = (int)(prazoAnos * 12);

        for (var m = 0; m < meses; m++)
        {
            // Aluguel reajusta anualmente por IPCA
            if (m > 0 && m % 12 == 0)
                aluguelMes *= (1m + ipcaAnual);

            gastoTotalAluguel += aluguelMes;

            // Investe a diferença (se parcela > aluguel)
            var sobra = parcelaMedia - aluguelMes;
            if (sobra > 0) patrimonioInvestimento += sobra;

            // Rende mensalmente
            patrimonioInvestimento *= (1m + taxaSelicMensal);
        }

        var patrimonioCompra = valorImovelFinal; // imóvel quitado
        var saldoLiquidoCompra = patrimonioCompra - gastoTotalCompra;
        var saldoLiquidoAluguel = patrimonioInvestimento - gastoTotalAluguel;

        return new AluguelVsCompraResult
        {
            PrazoAnos = (int)prazoAnos,
            ValorImovelInicial = valorImovel,
            ValorImovelFinal = Math.Round(valorImovelFinal, 2),
            Entrada = entrada,
            AluguelInicial = aluguelMensalAtual,
            AluguelFinal = Math.Round(aluguelMes, 2),
            GastoTotalComprar = Math.Round(gastoTotalCompra, 2),
            GastoTotalAlugar = Math.Round(gastoTotalAluguel, 2),
            PatrimonioFinalComprar = Math.Round(patrimonioCompra, 2),
            PatrimonioFinalAlugar = Math.Round(patrimonioInvestimento, 2),
            SaldoLiquidoComprar = Math.Round(saldoLiquidoCompra, 2),
            SaldoLiquidoAlugar = Math.Round(saldoLiquidoAluguel, 2),
            Recomendacao = saldoLiquidoCompra > saldoLiquidoAluguel
                ? "Comprar tende a ser mais vantajoso no cenário modelado."
                : "Alugar e investir tende a ser mais vantajoso no cenário modelado.",
            DiferencaAbsoluta = Math.Round(Math.Abs(saldoLiquidoCompra - saldoLiquidoAluguel), 2)
        };
    }

    // ========== Parcelamento direto com a incorporadora ==========

    public ParcelamentoDiretoResult CalcularParcelamentoDireto(
        decimal valorTotal,
        decimal entrada,
        int qtdParcelas,
        decimal taxaReajusteAnual = 0.08m)
    {
        if (valorTotal <= 0) throw new ArgumentException("Valor total inválido.");
        if (entrada < 0 || entrada >= valorTotal) throw new ArgumentException("Entrada inválida.");
        if (qtdParcelas <= 0) throw new ArgumentException("Parcelas inválidas.");

        var financiado = valorTotal - entrada;
        var parcelaInicial = financiado / qtdParcelas;

        var taxaMensal = (decimal)Math.Pow((double)(1 + taxaReajusteAnual), 1.0 / 12.0) - 1m;
        var parcelaFinal = parcelaInicial * (decimal)Math.Pow((double)(1 + taxaMensal), qtdParcelas - 1);

        decimal somaParcelas = 0;
        for (var i = 0; i < qtdParcelas; i++)
        {
            somaParcelas += parcelaInicial * (decimal)Math.Pow((double)(1 + taxaMensal), i);
        }
        var custoTotal = entrada + somaParcelas;

        return new ParcelamentoDiretoResult
        {
            ValorTotal = valorTotal,
            Entrada = entrada,
            QtdParcelas = qtdParcelas,
            TaxaReajusteAnualPct = Math.Round(taxaReajusteAnual * 100m, 2),
            ParcelaInicial = Math.Round(parcelaInicial, 2),
            ParcelaFinal = Math.Round(parcelaFinal, 2),
            CustoTotal = Math.Round(custoTotal, 2),
            JurosTotais = Math.Round(custoTotal - valorTotal, 2)
        };
    }

    // ========== Simulação agregada (tudo em uma) ==========

    public SimulacaoCompletaResult SimulacaoCompleta(SimulacaoCompletaRequest req)
    {
        var sfh = CalcularSFH(req.ValorImovel, req.Entrada, req.PrazoAnos, req.TaxaSfhAnual);
        var sfi = CalcularSFI(req.ValorImovel, req.Entrada, req.PrazoAnos);
        var impostos = CalcularImpostos(req.ValorImovel, req.Uf ?? "SP");
        var capacidade = req.RendaMensal > 0
            ? CalcularCapacidade(req.RendaMensal, req.OutrasDividas)
            : null;
        var aluguelVsCompra = req.AluguelAtual > 0
            ? CompararAluguelVsCompra(req.ValorImovel, req.Entrada, req.AluguelAtual,
                prazoAnos: req.PrazoAnos, taxaSfhAnual: req.TaxaSfhAnual)
            : null;
        var parcelamento = req.QtdParcelasDireto > 0
            ? CalcularParcelamentoDireto(req.ValorImovel, req.Entrada, req.QtdParcelasDireto)
            : null;

        return new SimulacaoCompletaResult
        {
            ValorImovel = req.ValorImovel,
            Entrada = req.Entrada,
            Sfh = sfh,
            Sfi = sfi,
            Impostos = impostos,
            Capacidade = capacidade,
            AluguelVsCompra = aluguelVsCompra,
            ParcelamentoDireto = parcelamento,
            ParcelaCabe = capacidade is null ? (bool?)null
                : sfh.PrimeiraParcela <= capacidade.ParcelaMaxima30Pct,
            ResumoExecutivo = GerarResumo(sfh, impostos, capacidade)
        };
    }

    private string GerarResumo(FinanciamentoResult sfh, ImpostosCompraResult impostos, CapacidadePagamentoResult? capacidade)
    {
        var parcela = sfh.PrimeiraParcela.ToString("N2");
        var impostosTotal = impostos.Total.ToString("N2");

        var resumo = $"Primeira parcela SFH: R$ {parcela}. Impostos de compra: R$ {impostosTotal}.";
        if (capacidade is not null)
        {
            resumo += capacidade.ParcelaMaxima30Pct >= sfh.PrimeiraParcela
                ? " A parcela cabe no orçamento (regra dos 30%)."
                : $" A parcela ultrapassa o limite de 30% da renda (máx R$ {capacidade.ParcelaMaxima30Pct:N2}).";
        }
        return resumo;
    }
}

// ========== DTOs ==========

public class CapacidadePagamentoResult
{
    public decimal RendaMensal { get; set; }
    public decimal OutrasDividas { get; set; }
    public decimal ParcelaMaxima30Pct { get; set; }
    public decimal ParcelaMaxima35Pct { get; set; }
    public decimal ImovelAproximadoSFH { get; set; }
    public string? Alerta { get; set; }
}

public class FinanciamentoResult
{
    public string Sistema { get; set; } = string.Empty;
    public decimal ValorImovel { get; set; }
    public decimal Entrada { get; set; }
    public decimal ValorFinanciado { get; set; }
    public decimal PctEntrada { get; set; }
    public int PrazoMeses { get; set; }
    public decimal TaxaAnualPct { get; set; }
    public decimal PrimeiraParcela { get; set; }
    public decimal UltimaParcela { get; set; }
    public decimal JurosTotais { get; set; }
    public decimal CustoTotal { get; set; }
    public decimal Cet { get; set; }
}

public class ImpostosCompraResult
{
    public string Uf { get; set; } = string.Empty;
    public decimal ValorImovel { get; set; }
    public decimal ItbiPct { get; set; }
    public decimal Itbi { get; set; }
    public decimal Cartorio { get; set; }
    public decimal Total { get; set; }
    public decimal PctSobreImovel { get; set; }
}

public class AluguelVsCompraResult
{
    public int PrazoAnos { get; set; }
    public decimal ValorImovelInicial { get; set; }
    public decimal ValorImovelFinal { get; set; }
    public decimal Entrada { get; set; }
    public decimal AluguelInicial { get; set; }
    public decimal AluguelFinal { get; set; }
    public decimal GastoTotalComprar { get; set; }
    public decimal GastoTotalAlugar { get; set; }
    public decimal PatrimonioFinalComprar { get; set; }
    public decimal PatrimonioFinalAlugar { get; set; }
    public decimal SaldoLiquidoComprar { get; set; }
    public decimal SaldoLiquidoAlugar { get; set; }
    public decimal DiferencaAbsoluta { get; set; }
    public string Recomendacao { get; set; } = string.Empty;
}

public class ParcelamentoDiretoResult
{
    public decimal ValorTotal { get; set; }
    public decimal Entrada { get; set; }
    public int QtdParcelas { get; set; }
    public decimal TaxaReajusteAnualPct { get; set; }
    public decimal ParcelaInicial { get; set; }
    public decimal ParcelaFinal { get; set; }
    public decimal CustoTotal { get; set; }
    public decimal JurosTotais { get; set; }
}

public class SimulacaoCompletaRequest
{
    public decimal ValorImovel { get; set; }
    public decimal Entrada { get; set; }
    public decimal PrazoAnos { get; set; } = 30;
    public decimal TaxaSfhAnual { get; set; } = 0.0975m;
    public string? Uf { get; set; } = "SP";
    public decimal RendaMensal { get; set; }
    public decimal OutrasDividas { get; set; }
    public decimal AluguelAtual { get; set; }
    public int QtdParcelasDireto { get; set; }
}

public class SimulacaoCompletaResult
{
    public decimal ValorImovel { get; set; }
    public decimal Entrada { get; set; }
    public FinanciamentoResult Sfh { get; set; } = new();
    public FinanciamentoResult Sfi { get; set; } = new();
    public ImpostosCompraResult Impostos { get; set; } = new();
    public CapacidadePagamentoResult? Capacidade { get; set; }
    public AluguelVsCompraResult? AluguelVsCompra { get; set; }
    public ParcelamentoDiretoResult? ParcelamentoDireto { get; set; }
    public bool? ParcelaCabe { get; set; }
    public string ResumoExecutivo { get; set; } = string.Empty;
}
