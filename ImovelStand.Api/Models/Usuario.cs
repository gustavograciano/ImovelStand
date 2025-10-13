using System.ComponentModel.DataAnnotations;

namespace ImovelStand.Api.Models;

public class Usuario
{
    [Key]
    public int Id { get; set; }

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
    public string Role { get; set; } = "Corretor"; // Admin, Corretor

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public bool Ativo { get; set; } = true;
}
