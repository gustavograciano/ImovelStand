using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;
using ImovelStand.Domain.Enums;

namespace ImovelStand.Domain.Entities;

public class Comissao : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public int VendaId { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    [Required]
    public TipoComissao Tipo { get; set; }

    [Required]
    public decimal Percentual { get; set; }

    [Required]
    public decimal Valor { get; set; }

    [Required]
    public StatusComissao Status { get; set; } = StatusComissao.Pendente;

    public DateTime? DataAprovacao { get; set; }
    public DateTime? DataPagamento { get; set; }

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(VendaId))]
    public virtual Venda Venda { get; set; } = null!;

    [ForeignKey(nameof(UsuarioId))]
    public virtual Usuario Usuario { get; set; } = null!;
}
