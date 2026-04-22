using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

public class HistoricoPreco : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public int ApartamentoId { get; set; }

    [Required]
    public decimal PrecoAnterior { get; set; }

    [Required]
    public decimal PrecoNovo { get; set; }

    [MaxLength(500)]
    public string? Motivo { get; set; }

    public int? AlteradoPorUsuarioId { get; set; }

    [Required]
    public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ApartamentoId))]
    public virtual Apartamento Apartamento { get; set; } = null!;
}
