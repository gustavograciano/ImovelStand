using ImovelStand.Domain.Enums;

namespace ImovelStand.Application.Dtos;

public class EnderecoDto
{
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
}

public class EmpreendimentoCreateRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Construtora { get; set; }
    public EnderecoDto Endereco { get; set; } = new();
    public DateTime? DataLancamento { get; set; }
    public DateTime? DataEntregaPrevista { get; set; }
    public StatusEmpreendimento Status { get; set; } = StatusEmpreendimento.PreLancamento;
    public decimal? VgvEstimado { get; set; }
}

public class EmpreendimentoUpdateRequest : EmpreendimentoCreateRequest { }

public class EmpreendimentoResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Construtora { get; set; }
    public EnderecoDto Endereco { get; set; } = new();
    public DateTime? DataLancamento { get; set; }
    public DateTime? DataEntregaPrevista { get; set; }
    public StatusEmpreendimento Status { get; set; }
    public decimal? VgvEstimado { get; set; }
    public DateTime DataCadastro { get; set; }
}
