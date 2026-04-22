using System.ComponentModel.DataAnnotations;

namespace ImovelStand.Domain.Entities;

public class Plano
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public decimal PrecoMensal { get; set; }

    [Required]
    public int MaxEmpreendimentos { get; set; }

    [Required]
    public int MaxUnidades { get; set; }

    [Required]
    public int MaxUsuarios { get; set; }

    [MaxLength(2000)]
    public string? FeaturesJson { get; set; }

    public bool Ativo { get; set; } = true;
}
