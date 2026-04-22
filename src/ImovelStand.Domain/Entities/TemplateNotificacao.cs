using System.ComponentModel.DataAnnotations;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

public enum CanalNotificacao
{
    Email = 0,
    Whatsapp = 1
}

public class TemplateNotificacao : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    /// <summary>Código identificador imutável (ex: "reserva.expirou", "proposta.enviada").</summary>
    [Required]
    [MaxLength(100)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    public CanalNotificacao Canal { get; set; }

    [Required]
    [MaxLength(200)]
    public string Assunto { get; set; } = string.Empty;

    /// <summary>Corpo com placeholders {{ cliente.nome }}, {{ apto.numero }}, etc.</summary>
    [Required]
    [MaxLength(4000)]
    public string Corpo { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
