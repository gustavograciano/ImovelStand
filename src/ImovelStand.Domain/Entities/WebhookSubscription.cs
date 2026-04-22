using System.ComponentModel.DataAnnotations;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

public class WebhookSubscription : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Evento { get; set; } = string.Empty; // venda.criada, venda.aprovada, etc

    [MaxLength(500)]
    public string? Secret { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int FalhasConsecutivas { get; set; }
    public DateTime? UltimoDisparoEm { get; set; }
    public DateTime? UltimoSucessoEm { get; set; }
}
