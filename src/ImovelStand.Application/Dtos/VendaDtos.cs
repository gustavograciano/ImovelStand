using ImovelStand.Domain.Enums;

namespace ImovelStand.Application.Dtos;

public class VendaCreateRequest
{
    public int? PropostaId { get; set; }
    public int ClienteId { get; set; }
    public int ApartamentoId { get; set; }
    public int CorretorId { get; set; }
    public int? CorretorCaptacaoId { get; set; }
    public decimal ValorFinal { get; set; }
    public CondicaoPagamentoDto CondicaoFinal { get; set; } = new();
    public string? Observacoes { get; set; }
}

public class VendaResponse
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int? PropostaId { get; set; }
    public int ClienteId { get; set; }
    public string? ClienteNome { get; set; }
    public int ApartamentoId { get; set; }
    public string? ApartamentoNumero { get; set; }
    public int CorretorId { get; set; }
    public string? CorretorNome { get; set; }
    public int? CorretorCaptacaoId { get; set; }
    public int? GerenteAprovadorId { get; set; }
    public DateTime DataFechamento { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public decimal ValorFinal { get; set; }
    public StatusVenda Status { get; set; }
    public string? ContratoUrl { get; set; }
    public string? Observacoes { get; set; }
    public CondicaoPagamentoDto CondicaoFinal { get; set; } = new();
    public List<ComissaoResponse> Comissoes { get; set; } = new();
}

public class ComissaoResponse
{
    public int Id { get; set; }
    public int VendaId { get; set; }
    public int UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
    public TipoComissao Tipo { get; set; }
    public decimal Percentual { get; set; }
    public decimal Valor { get; set; }
    public StatusComissao Status { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public DateTime? DataPagamento { get; set; }
}

public class ComissaoOverrideRequest
{
    public int ComissaoId { get; set; }
    public decimal NovoPercentual { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
