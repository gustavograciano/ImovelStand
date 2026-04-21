using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImovelStand.Domain.Entities;

public class Torre
{
    [Key]
    public int Id { get; set; }

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
