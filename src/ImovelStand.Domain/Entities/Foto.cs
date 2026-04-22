using System.ComponentModel.DataAnnotations;
using ImovelStand.Domain.Abstractions;
using ImovelStand.Domain.Enums;

namespace ImovelStand.Domain.Entities;

public class Foto : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public TipoEntidadeFoto EntidadeTipo { get; set; }

    [Required]
    public int EntidadeId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? ThumbnailUrl { get; set; }

    public int Ordem { get; set; }

    [MaxLength(500)]
    public string? Legenda { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
}
