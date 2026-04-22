using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

public class ClienteDependente : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public int ClienteId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(14)]
    public string? Cpf { get; set; }

    public DateTime? DataNascimento { get; set; }

    [MaxLength(50)]
    public string? Parentesco { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ClienteId))]
    public virtual Cliente Cliente { get; set; } = null!;
}
