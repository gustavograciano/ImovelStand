using System.ComponentModel.DataAnnotations;

namespace ImovelStand.Api.Models;

public class Apartamento
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Numero { get; set; } = string.Empty;

    [Required]
    public int Andar { get; set; }

    [Required]
    public int Quartos { get; set; }

    [Required]
    public int Banheiros { get; set; }

    [Required]
    public decimal AreaMetrosQuadrados { get; set; }

    [Required]
    public decimal Preco { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Disponível"; // Disponível, Reservado, Vendido

    [MaxLength(500)]
    public string? Descricao { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    // Relacionamentos
    public virtual ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
