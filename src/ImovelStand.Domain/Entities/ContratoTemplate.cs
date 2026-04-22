using System.ComponentModel.DataAnnotations;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

public class ContratoTemplate : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Descricao { get; set; }

    /// <summary>Chave do arquivo DOCX no IFileStorage.</summary>
    [Required]
    [MaxLength(1000)]
    public string ArquivoKey { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
