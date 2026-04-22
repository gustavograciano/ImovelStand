using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;
using ImovelStand.Domain.Enums;

namespace ImovelStand.Domain.Entities;

public class Apartamento : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public int TorreId { get; set; }

    [Required]
    public int TipologiaId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Numero { get; set; } = string.Empty;

    [Required]
    public int Pavimento { get; set; }

    public Orientacao? Orientacao { get; set; }

    [Required]
    public decimal PrecoAtual { get; set; }

    [Required]
    public StatusApartamento Status { get; set; } = StatusApartamento.Disponivel;

    [MaxLength(1000)]
    public string? Observacoes { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(TorreId))]
    public virtual Torre Torre { get; set; } = null!;

    [ForeignKey(nameof(TipologiaId))]
    public virtual Tipologia Tipologia { get; set; } = null!;

    public virtual ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public virtual ICollection<HistoricoPreco> HistoricoPrecos { get; set; } = new List<HistoricoPreco>();
}
