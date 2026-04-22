using System.ComponentModel.DataAnnotations;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

public class Usuario : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string SenhaHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "Corretor"; // Admin, Corretor, Gerente

    [MaxLength(20)]
    public string? Creci { get; set; }

    /// <summary>Percentual padrão de comissão desse corretor (ex: 0.03 = 3%).</summary>
    public decimal? PercentualComissao { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public DateTime? UltimoLoginEm { get; set; }

    public bool Ativo { get; set; } = true;
}
