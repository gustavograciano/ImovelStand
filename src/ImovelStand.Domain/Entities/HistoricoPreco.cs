using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImovelStand.Domain.Entities;

public class HistoricoPreco
{
    [Key]
    public int Id { get; set; }

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
