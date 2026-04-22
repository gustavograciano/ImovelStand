using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;
using ImovelStand.Domain.Enums;
using ImovelStand.Domain.ValueObjects;

namespace ImovelStand.Domain.Entities;

public class Proposta : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(30)]
    public string Numero { get; set; } = string.Empty;

    [Required]
    public int ClienteId { get; set; }

    [Required]
    public int ApartamentoId { get; set; }

    [Required]
    public int CorretorId { get; set; }

    /// <summary>Versão linear da proposta (contrapropostas incrementam).</summary>
    public int Versao { get; set; } = 1;

    public int? PropostaOriginalId { get; set; }

    [Required]
    public decimal ValorOferecido { get; set; }

    [Required]
    public StatusProposta Status { get; set; } = StatusProposta.Rascunho;

    public DateTime? DataEnvio { get; set; }

    public DateTime? DataValidade { get; set; }

    public DateTime? DataRespostaCliente { get; set; }

    [MaxLength(2000)]
    public string? Observacoes { get; set; }

    public CondicaoPagamento Condicao { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(ClienteId))]
    public virtual Cliente Cliente { get; set; } = null!;

    [ForeignKey(nameof(ApartamentoId))]
    public virtual Apartamento Apartamento { get; set; } = null!;

    [ForeignKey(nameof(CorretorId))]
    public virtual Usuario Corretor { get; set; } = null!;

    [ForeignKey(nameof(PropostaOriginalId))]
    public virtual Proposta? PropostaOriginal { get; set; }

    public virtual ICollection<PropostaHistoricoStatus> Historico { get; set; } = new List<PropostaHistoricoStatus>();
}

public class PropostaHistoricoStatus : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public int PropostaId { get; set; }

    [Required]
    public StatusProposta StatusAnterior { get; set; }

    [Required]
    public StatusProposta StatusNovo { get; set; }

    public int? AlteradoPorUsuarioId { get; set; }

    [MaxLength(500)]
    public string? Motivo { get; set; }

    [Required]
    public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(PropostaId))]
    public virtual Proposta Proposta { get; set; } = null!;
}
