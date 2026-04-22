using ImovelStand.Application.Services;
using ImovelStand.Domain.Enums;

namespace ImovelStand.Tests.Services;

public class PropostaStateMachineTests
{
    [Theory]
    [InlineData(StatusProposta.Rascunho, StatusProposta.Enviada, true)]
    [InlineData(StatusProposta.Rascunho, StatusProposta.Cancelada, true)]
    [InlineData(StatusProposta.Rascunho, StatusProposta.Aceita, false)] // não pula
    [InlineData(StatusProposta.Enviada, StatusProposta.ContrapropostaCliente, true)]
    [InlineData(StatusProposta.Enviada, StatusProposta.Aceita, true)]
    [InlineData(StatusProposta.Enviada, StatusProposta.Expirada, true)]
    [InlineData(StatusProposta.ContrapropostaCliente, StatusProposta.ContrapropostaCorretor, true)]
    [InlineData(StatusProposta.ContrapropostaCliente, StatusProposta.Rascunho, false)]
    [InlineData(StatusProposta.Aceita, StatusProposta.Reprovada, false)] // terminal
    [InlineData(StatusProposta.Cancelada, StatusProposta.Enviada, false)] // terminal
    public void PodeTransicionar(StatusProposta de, StatusProposta para, bool esperado)
    {
        Assert.Equal(esperado, PropostaStateMachine.PodeTransicionar(de, para));
    }

    [Fact]
    public void Garantir_TransicaoInvalida_Lanca()
    {
        Assert.Throws<InvalidPropostaTransitionException>(() =>
            PropostaStateMachine.Garantir(StatusProposta.Rascunho, StatusProposta.Aceita));
    }

    [Theory]
    [InlineData(StatusProposta.Aceita, true)]
    [InlineData(StatusProposta.Reprovada, true)]
    [InlineData(StatusProposta.Expirada, true)]
    [InlineData(StatusProposta.Cancelada, true)]
    [InlineData(StatusProposta.Rascunho, false)]
    [InlineData(StatusProposta.Enviada, false)]
    public void EstadoTerminal(StatusProposta status, bool esperado)
    {
        Assert.Equal(esperado, PropostaStateMachine.EstadoTerminal(status));
    }
}
