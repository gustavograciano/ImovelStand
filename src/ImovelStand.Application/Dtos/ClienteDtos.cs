using ImovelStand.Domain.Enums;

namespace ImovelStand.Application.Dtos;

public class ClienteCreateRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string? Rg { get; set; }
    public DateTime? DataNascimento { get; set; }
    public EstadoCivil? EstadoCivil { get; set; }
    public RegimeBens? RegimeBens { get; set; }
    public string? Profissao { get; set; }
    public string? Empresa { get; set; }
    public decimal? RendaMensal { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Whatsapp { get; set; }
    public EnderecoDto? Endereco { get; set; }
    public OrigemLead? OrigemLead { get; set; }
    public int? CorretorResponsavelId { get; set; }
    public bool ConsentimentoLgpd { get; set; }
}

public class ClienteUpdateRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Rg { get; set; }
    public DateTime? DataNascimento { get; set; }
    public EstadoCivil? EstadoCivil { get; set; }
    public RegimeBens? RegimeBens { get; set; }
    public string? Profissao { get; set; }
    public string? Empresa { get; set; }
    public decimal? RendaMensal { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Whatsapp { get; set; }
    public EnderecoDto? Endereco { get; set; }
    public OrigemLead? OrigemLead { get; set; }
    public StatusFunil StatusFunil { get; set; }
    public int? CorretorResponsavelId { get; set; }
    public int? ConjugeId { get; set; }
}

public class ClienteResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string? Rg { get; set; }
    public DateTime? DataNascimento { get; set; }
    public EstadoCivil? EstadoCivil { get; set; }
    public RegimeBens? RegimeBens { get; set; }
    public string? Profissao { get; set; }
    public string? Empresa { get; set; }
    public decimal? RendaMensal { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Whatsapp { get; set; }
    public EnderecoDto? Endereco { get; set; }
    public OrigemLead? OrigemLead { get; set; }
    public StatusFunil StatusFunil { get; set; }
    public int? CorretorResponsavelId { get; set; }
    public int? ConjugeId { get; set; }
    public bool ConsentimentoLgpd { get; set; }
    public DateTime? ConsentimentoLgpdEm { get; set; }
    public DateTime DataCadastro { get; set; }
}
