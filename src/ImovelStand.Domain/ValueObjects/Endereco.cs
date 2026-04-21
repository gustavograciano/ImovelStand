using System.ComponentModel.DataAnnotations;

namespace ImovelStand.Domain.ValueObjects;

public class Endereco
{
    [MaxLength(200)]
    public string Logradouro { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Numero { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Complemento { get; set; }

    [MaxLength(100)]
    public string Bairro { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Cidade { get; set; } = string.Empty;

    [MaxLength(2)]
    public string Uf { get; set; } = string.Empty;

    [MaxLength(9)]
    public string Cep { get; set; } = string.Empty;
}
