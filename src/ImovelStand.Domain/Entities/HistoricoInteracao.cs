using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;
using ImovelStand.Domain.Enums;

namespace ImovelStand.Domain.Entities;

public class HistoricoInteracao : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public int ClienteId { get; set; }

    public int? UsuarioId { get; set; }

    [Required]
    public TipoInteracao Tipo { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Conteudo { get; set; } = string.Empty;

    [Required]
    public DateTime DataHora { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ClienteId))]
    public virtual Cliente Cliente { get; set; } = null!;

    [ForeignKey(nameof(UsuarioId))]
    public virtual Usuario? Usuario { get; set; }
}
