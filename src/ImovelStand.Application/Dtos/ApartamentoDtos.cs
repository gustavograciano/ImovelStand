using ImovelStand.Domain.Enums;

namespace ImovelStand.Application.Dtos;

public class ApartamentoCreateRequest
{
    public int TorreId { get; set; }
    public int TipologiaId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int Pavimento { get; set; }
    public Orientacao? Orientacao { get; set; }
    public decimal PrecoAtual { get; set; }
    public string? Observacoes { get; set; }
}

public class ApartamentoUpdateRequest
{
    public int TipologiaId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int Pavimento { get; set; }
    public Orientacao? Orientacao { get; set; }
    public decimal PrecoAtual { get; set; }
    public StatusApartamento Status { get; set; }
    public string? Observacoes { get; set; }
}

public class ApartamentoResponse
{
    public int Id { get; set; }
    public int TorreId { get; set; }
    public string? TorreNome { get; set; }
    public int TipologiaId { get; set; }
    public string? TipologiaNome { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int Pavimento { get; set; }
    public Orientacao? Orientacao { get; set; }
    public decimal PrecoAtual { get; set; }
    public StatusApartamento Status { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCadastro { get; set; }
}

public class ApartamentoFiltro
{
    public StatusApartamento? Status { get; set; }
    public int? EmpreendimentoId { get; set; }
    public int? TorreId { get; set; }
    public int? TipologiaId { get; set; }
    public int? PavimentoMin { get; set; }
    public int? PavimentoMax { get; set; }
    public decimal? PrecoMin { get; set; }
    public decimal? PrecoMax { get; set; }
}
