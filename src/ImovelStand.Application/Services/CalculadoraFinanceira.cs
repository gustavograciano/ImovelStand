using ImovelStand.Domain.Enums;
using ImovelStand.Domain.ValueObjects;

namespace ImovelStand.Application.Services;

/// <summary>
/// Serviço de cálculo financeiro de condições de pagamento. Dado um ValorTotal
/// e alguns parâmetros (entrada, sinal, chaves, parcelas), distribui o restante
/// automaticamente entre as parcelas mensais.
/// </summary>
public class CalculadoraFinanceira
{
    /// <summary>
    /// Dada uma CondicaoPagamento com <c>ValorTotal</c> + <c>Entrada</c> + <c>Sinal</c>
    /// + <c>ValorChaves</c> + <c>QtdParcelasMensais</c> + <c>QtdSemestrais/ValorSemestral</c>,
    /// calcula <c>ValorParcelaMensal</c> para fechar o valor total.
    /// </summary>
    public CondicaoPagamento Distribuir(CondicaoPagamento condicao)
    {
        if (condicao.ValorTotal <= 0)
            throw new ArgumentException("ValorTotal deve ser positivo.", nameof(condicao));

        var fixados = condicao.Entrada
                    + condicao.Sinal
                    + condicao.ValorChaves
                    + condicao.QtdSemestrais * condicao.ValorSemestral
                    + condicao.QtdPosChaves * condicao.ValorPosChaves;

        var restante = condicao.ValorTotal - fixados;

        if (restante < 0)
            throw new InvalidOperationException("Soma de entrada + sinal + chaves + semestrais + pós-chaves excede o valor total.");

        if (condicao.QtdParcelasMensais > 0)
        {
            condicao.ValorParcelaMensal = Math.Round(restante / condicao.QtdParcelasMensais, 2, MidpointRounding.ToEven);
        }
        else if (restante > 0.01m)
        {
            throw new InvalidOperationException("Parcelas mensais == 0 mas restante > 0 — reveja a condição.");
        }

        return condicao;
    }

    /// <summary>
    /// Gera lista projetada de pagamentos mês-a-mês, útil para espelho.
    /// Aplica índice de reajuste sobre parcelas futuras (aproximação mensal).
    /// </summary>
    public IReadOnlyList<ParcelaProjetada> ProjetarParcelas(
        CondicaoPagamento condicao,
        DateTime? dataReferencia = null,
        decimal taxaIndiceAnual = 0.08m)
    {
        var parcelas = new List<ParcelaProjetada>();
        var hoje = dataReferencia ?? DateTime.UtcNow;

        if (condicao.Entrada > 0)
            parcelas.Add(new ParcelaProjetada(condicao.EntradaData ?? hoje, "Entrada", condicao.Entrada));

        if (condicao.Sinal > 0)
            parcelas.Add(new ParcelaProjetada(condicao.SinalData ?? hoje.AddDays(30), "Sinal", condicao.Sinal));

        var taxaMensal = condicao.Indice == IndiceReajuste.SemReajuste
            ? 0m
            : (decimal)Math.Pow((double)(1 + taxaIndiceAnual), 1.0 / 12.0) - 1m;

        var primeiraParcela = condicao.PrimeiraParcelaData ?? hoje.AddMonths(1);
        for (var i = 0; i < condicao.QtdParcelasMensais; i++)
        {
            var valor = condicao.ValorParcelaMensal * (decimal)Math.Pow((double)(1 + taxaMensal), i);
            parcelas.Add(new ParcelaProjetada(
                primeiraParcela.AddMonths(i),
                $"Parcela {i + 1}/{condicao.QtdParcelasMensais}",
                Math.Round(valor, 2)));
        }

        // Semestrais: a cada 6 meses a partir da primeira parcela
        for (var i = 0; i < condicao.QtdSemestrais; i++)
        {
            parcelas.Add(new ParcelaProjetada(
                primeiraParcela.AddMonths((i + 1) * 6),
                $"Semestral {i + 1}/{condicao.QtdSemestrais}",
                condicao.ValorSemestral));
        }

        if (condicao.ValorChaves > 0 && condicao.ChavesDataPrevista.HasValue)
            parcelas.Add(new ParcelaProjetada(condicao.ChavesDataPrevista.Value, "Chaves", condicao.ValorChaves));

        var baseChaves = condicao.ChavesDataPrevista ?? primeiraParcela.AddMonths(condicao.QtdParcelasMensais);
        for (var i = 0; i < condicao.QtdPosChaves; i++)
        {
            parcelas.Add(new ParcelaProjetada(
                baseChaves.AddMonths(i + 1),
                $"Pós-chaves {i + 1}/{condicao.QtdPosChaves}",
                condicao.ValorPosChaves));
        }

        return parcelas.OrderBy(p => p.Data).ToList();
    }
}

public record ParcelaProjetada(DateTime Data, string Descricao, decimal Valor);
