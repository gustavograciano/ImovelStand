using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

public class Torre : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public int EmpreendimentoId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public int Pavimentos { get; set; }

    [Required]
    public int ApartamentosPorPavimento { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(EmpreendimentoId))]
    public virtual Empreendimento Empreendimento { get; set; } = null!;

    public virtual ICollection<Apartamento> Apartamentos { get; set; } = new List<Apartamento>();
}
