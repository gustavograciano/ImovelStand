using System.ComponentModel.DataAnnotations;
using ImovelStand.Domain.Abstractions;
using ImovelStand.Domain.Enums;
using ImovelStand.Domain.ValueObjects;

namespace ImovelStand.Domain.Entities;

public class Empreendimento : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(220)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Descricao { get; set; }

    [MaxLength(200)]
    public string? Construtora { get; set; }

    public Endereco Endereco { get; set; } = new();

    public DateTime? DataLancamento { get; set; }

    public DateTime? DataEntregaPrevista { get; set; }

    [Required]
    public StatusEmpreendimento Status { get; set; } = StatusEmpreendimento.PreLancamento;

    public decimal? VgvEstimado { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Torre> Torres { get; set; } = new List<Torre>();
    public virtual ICollection<Tipologia> Tipologias { get; set; } = new List<Tipologia>();
}
