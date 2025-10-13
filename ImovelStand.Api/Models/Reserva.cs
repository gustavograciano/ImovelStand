using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImovelStand.Api.Models;

public class Reserva
{
    [Key]
    public int Id { get; set; }

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
