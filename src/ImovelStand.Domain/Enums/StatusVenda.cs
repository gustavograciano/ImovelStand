namespace ImovelStand.Domain.Enums;

public enum StatusVenda
{
    Negociada = 0,
    EmContrato = 1,
    Assinada = 2,
    Cancelada = 3,
    Distratada = 4
}

public enum TipoComissao
{
    Captacao = 0,
    Venda = 1,
    OverrideGerente = 2,
    Parceria = 3
}

public enum StatusComissao
{
    Pendente = 0,
    Aprovada = 1,
    Paga = 2,
    Cancelada = 3
}
