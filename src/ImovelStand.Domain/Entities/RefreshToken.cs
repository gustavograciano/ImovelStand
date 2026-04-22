using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

public class RefreshToken : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    [Required]
    [MaxLength(200)]
    public string TokenHash { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiraEm { get; set; }

    public DateTime? RevogadoEm { get; set; }

    [MaxLength(200)]
    public string? MotivoRevogacao { get; set; }

    [MaxLength(45)]
    public string? IpCriacao { get; set; }

    [MaxLength(500)]
    public string? UserAgentCriacao { get; set; }

    public int? SubstituidoPorTokenId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UsuarioId))]
    public virtual Usuario Usuario { get; set; } = null!;

    public bool EstaAtivo => RevogadoEm == null && ExpiraEm > DateTime.UtcNow;
}
