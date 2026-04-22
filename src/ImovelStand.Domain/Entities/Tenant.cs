using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImovelStand.Domain.Entities;

public class Tenant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(18)]
    public string? Cnpj { get; set; }

    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    public int PlanoId { get; set; }

    public DateTime? TrialAte { get; set; }

    public DateTime? AtivoAte { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(PlanoId))]
    public virtual Plano Plano { get; set; } = null!;
}
