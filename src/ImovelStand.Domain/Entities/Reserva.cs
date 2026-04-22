using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

public class Reserva : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public int ClienteId { get; set; }

    [Required]
    public int ApartamentoId { get; set; }

    [Required]
    public DateTime DataReserva { get; set; } = DateTime.UtcNow;

    public DateTime? DataExpiracao { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Ativa"; // Ativa, Expirada, Cancelada, Confirmada

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    // Relacionamentos
    [ForeignKey("ClienteId")]
    public virtual Cliente Cliente { get; set; } = null!;

    [ForeignKey("ApartamentoId")]
    public virtual Apartamento Apartamento { get; set; } = null!;
}
