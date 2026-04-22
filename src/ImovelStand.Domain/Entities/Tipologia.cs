using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

public class Tipologia : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public int EmpreendimentoId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public decimal AreaPrivativa { get; set; }

    [Required]
    public decimal AreaTotal { get; set; }

    [Required]
    public int Quartos { get; set; }

    public int Suites { get; set; }

    [Required]
    public int Banheiros { get; set; }

    public int Vagas { get; set; }

    [Required]
    public decimal PrecoBase { get; set; }

    [MaxLength(500)]
    public string? PlantaUrl { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(EmpreendimentoId))]
    public virtual Empreendimento Empreendimento { get; set; } = null!;

    public virtual ICollection<Apartamento> Apartamentos { get; set; } = new List<Apartamento>();
}
