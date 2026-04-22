namespace ImovelStand.Domain.Enums;

public enum StatusProposta
{
    Rascunho = 0,
    Enviada = 1,
    ContrapropostaCliente = 2,
    ContrapropostaCorretor = 3,
    Aceita = 4,
    Reprovada = 5,
    Expirada = 6,
    Cancelada = 7
}

public enum IndiceReajuste
{
    SemReajuste = 0,
    Incc = 1,
    Ipca = 2,
    Igpm = 3,
    Tr = 4,
    Selic = 5
}
