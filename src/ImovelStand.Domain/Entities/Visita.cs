using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

public class Visita : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public int ClienteId { get; set; }

    [Required]
    public int CorretorId { get; set; }

    [Required]
    public int EmpreendimentoId { get; set; }

    [Required]
    public DateTime DataHora { get; set; }

    public int? DuracaoMinutos { get; set; }

    [MaxLength(1000)]
    public string? Observacoes { get; set; }

    public bool GerouProposta { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ClienteId))]
    public virtual Cliente Cliente { get; set; } = null!;

    [ForeignKey(nameof(CorretorId))]
    public virtual Usuario Corretor { get; set; } = null!;

    [ForeignKey(nameof(EmpreendimentoId))]
    public virtual Empreendimento Empreendimento { get; set; } = null!;
}
