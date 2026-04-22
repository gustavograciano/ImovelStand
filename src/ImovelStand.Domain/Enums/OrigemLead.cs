namespace ImovelStand.Domain.Enums;

public enum OrigemLead
{
    Indicacao = 0,
    Facebook = 1,
    Instagram = 2,
    Google = 3,
    Plantao = 4,
    Site = 5,
    WhatsApp = 6,
    Evento = 7,
    Outros = 99
}

public enum StatusFunil
{
    Lead = 0,
    Contato = 1,
    Visita = 2,
    Proposta = 3,
    Negociacao = 4,
    Venda = 5,
    Descarte = 99
}

public enum EstadoCivil
{
    Solteiro = 0,
    Casado = 1,
    Divorciado = 2,
    Viuvo = 3,
    UniaoEstavel = 4,
    Separado = 5
}

public enum RegimeBens
{
    ComunhaoParcial = 0,
    ComunhaoUniversal = 1,
    SeparacaoTotal = 2,
    ParticipacaoFinalAquestos = 3
}

public enum TipoInteracao
{
    Ligacao = 0,
    Whatsapp = 1,
    Email = 2,
    ReuniaoPresencial = 3,
    ReuniaoVideo = 4,
    Visita = 5,
    MensagemInterna = 99
}
