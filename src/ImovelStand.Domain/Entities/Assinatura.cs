using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Enums;

namespace ImovelStand.Domain.Entities;

public enum StatusAssinatura
{
    Trial = 0,
    Ativa = 1,
    Inadimplente = 2,
    Suspensa = 3,
    Cancelada = 4
}

/// <summary>
/// Assinatura de um Tenant a um Plano, com integração Iugu. Trial 14 dias,
/// depois cobrança mensal recorrente.
/// </summary>
public class Assinatura
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public int PlanoId { get; set; }

    [MaxLength(100)]
    public string? IuguCustomerId { get; set; }

    [MaxLength(100)]
    public string? IuguSubscriptionId { get; set; }

    [Required]
    public StatusAssinatura Status { get; set; } = StatusAssinatura.Trial;

    [Required]
    public DateTime DataInicio { get; set; } = DateTime.UtcNow;

    public DateTime? TrialFimEm { get; set; }

    public DateTime? ProximaCobranca { get; set; }

    public DateTime? CanceladaEm { get; set; }

    [MaxLength(500)]
    public string? MotivoCancelamento { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;

    [ForeignKey(nameof(PlanoId))]
    public virtual Plano Plano { get; set; } = null!;

    public bool EstaAtiva => Status is StatusAssinatura.Trial or StatusAssinatura.Ativa;
}
