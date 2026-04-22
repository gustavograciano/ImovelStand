using ImovelStand.Domain.Enums;

namespace ImovelStand.Domain.ValueObjects;

/// <summary>
/// Condição de pagamento de um imóvel: entrada + sinal + parcelas mensais +
/// semestrais + chaves + pós-chaves, com índice de reajuste.
/// Usado como owned type em <c>Proposta</c> e <c>Venda</c> (snapshot).
/// </summary>
public class CondicaoPagamento
{
    public decimal ValorTotal { get; set; }

    public decimal Entrada { get; set; }
    public DateTime? EntradaData { get; set; }

    public decimal Sinal { get; set; }
    public DateTime? SinalData { get; set; }

    public int QtdParcelasMensais { get; set; }
    public decimal ValorParcelaMensal { get; set; }
    public DateTime? PrimeiraParcelaData { get; set; }

    public int QtdSemestrais { get; set; }
    public decimal ValorSemestral { get; set; }

    public decimal ValorChaves { get; set; }
    public DateTime? ChavesDataPrevista { get; set; }

    public int QtdPosChaves { get; set; }
    public decimal ValorPosChaves { get; set; }

    public IndiceReajuste Indice { get; set; } = IndiceReajuste.Incc;
    public decimal TaxaJurosAnual { get; set; }

    public decimal ValorPagoTotal =>
        Entrada + Sinal
        + (QtdParcelasMensais * ValorParcelaMensal)
        + (QtdSemestrais * ValorSemestral)
        + ValorChaves
        + (QtdPosChaves * ValorPosChaves);
}
