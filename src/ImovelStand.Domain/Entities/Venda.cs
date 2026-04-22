using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

public class Venda : ITenantEntity
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
    public DateTime DataVenda { get; set; } = DateTime.UtcNow;

    [Required]
    public decimal ValorVenda { get; set; }

    [Required]
    public decimal ValorEntrada { get; set; }

    [Required]
    [MaxLength(50)]
    public string FormaPagamento { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Concluída"; // Concluída, Cancelada

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    // Relacionamentos
    [ForeignKey("ClienteId")]
    public virtual Cliente Cliente { get; set; } = null!;

    [ForeignKey("ApartamentoId")]
    public virtual Apartamento Apartamento { get; set; } = null!;
}
