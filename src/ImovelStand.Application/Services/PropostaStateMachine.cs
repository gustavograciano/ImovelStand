using ImovelStand.Domain.Enums;

namespace ImovelStand.Application.Services;

/// <summary>
/// State machine centralizada para <c>Proposta</c>. Transições inválidas lançam
/// <see cref="InvalidPropostaTransitionException"/>; tentar bypassar isso em
/// controller é o que vai quebrar o produto.
/// </summary>
public static class PropostaStateMachine
{
    private static readonly Dictionary<StatusProposta, StatusProposta[]> Transicoes = new()
    {
        [StatusProposta.Rascunho] = new[]
        {
            StatusProposta.Enviada,
            StatusProposta.Cancelada
        },
        [StatusProposta.Enviada] = new[]
        {
            StatusProposta.ContrapropostaCliente,
            StatusProposta.Aceita,
            StatusProposta.Reprovada,
            StatusProposta.Expirada,
            StatusProposta.Cancelada
        },
        [StatusProposta.ContrapropostaCliente] = new[]
        {
            StatusProposta.ContrapropostaCorretor,
            StatusProposta.Aceita,
            StatusProposta.Reprovada,
            StatusProposta.Cancelada
        },
        [StatusProposta.ContrapropostaCorretor] = new[]
        {
            StatusProposta.ContrapropostaCliente,
            StatusProposta.Aceita,
            StatusProposta.Reprovada,
            StatusProposta.Cancelada
        },
        // Estados terminais — não saem mais
        [StatusProposta.Aceita] = Array.Empty<StatusProposta>(),
        [StatusProposta.Reprovada] = Array.Empty<StatusProposta>(),
        [StatusProposta.Expirada] = Array.Empty<StatusProposta>(),
        [StatusProposta.Cancelada] = Array.Empty<StatusProposta>(),
    };

    public static bool PodeTransicionar(StatusProposta de, StatusProposta para) =>
        Transicoes.TryGetValue(de, out var permitidos) && permitidos.Contains(para);

    public static void Garantir(StatusProposta de, StatusProposta para)
    {
        if (!PodeTransicionar(de, para))
            throw new InvalidPropostaTransitionException(de, para);
    }

    public static bool EstadoTerminal(StatusProposta status) =>
        status is StatusProposta.Aceita
              or StatusProposta.Reprovada
              or StatusProposta.Expirada
              or StatusProposta.Cancelada;
}

public class InvalidPropostaTransitionException : InvalidOperationException
{
    public InvalidPropostaTransitionException(StatusProposta de, StatusProposta para)
        : base($"Transição de proposta inválida: {de} → {para}.")
    {
        De = de;
        Para = para;
    }

    public StatusProposta De { get; }
    public StatusProposta Para { get; }
}
