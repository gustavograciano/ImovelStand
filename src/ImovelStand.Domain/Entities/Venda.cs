using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;
using ImovelStand.Domain.Enums;
using ImovelStand.Domain.ValueObjects;

namespace ImovelStand.Domain.Entities;

public class Venda : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(30)]
    public string Numero { get; set; } = string.Empty;

    public int? PropostaId { get; set; }

    [Required]
    public int ClienteId { get; set; }

    [Required]
    public int ApartamentoId { get; set; }

    [Required]
    public int CorretorId { get; set; }

    /// <summary>Corretor que captou o lead (pode ser diferente do fechador).</summary>
    public int? CorretorCaptacaoId { get; set; }

    public int? GerenteAprovadorId { get; set; }

    [Required]
    public DateTime DataFechamento { get; set; } = DateTime.UtcNow;

    public DateTime? DataAprovacao { get; set; }

    [Required]
    public decimal ValorFinal { get; set; }

    [Required]
    public StatusVenda Status { get; set; } = StatusVenda.Negociada;

    [MaxLength(1000)]
    public string? ContratoUrl { get; set; }

    [MaxLength(1000)]
    public string? Observacoes { get; set; }

    /// <summary>Snapshot congelado da condição no momento da venda; proposta pode ser editada depois.</summary>
    public CondicaoPagamento CondicaoFinal { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(PropostaId))]
    public virtual Proposta? Proposta { get; set; }

    [ForeignKey(nameof(ClienteId))]
    public virtual Cliente Cliente { get; set; } = null!;

    [ForeignKey(nameof(ApartamentoId))]
    public virtual Apartamento Apartamento { get; set; } = null!;

    [ForeignKey(nameof(CorretorId))]
    public virtual Usuario Corretor { get; set; } = null!;

    [ForeignKey(nameof(CorretorCaptacaoId))]
    public virtual Usuario? CorretorCaptacao { get; set; }

    [ForeignKey(nameof(GerenteAprovadorId))]
    public virtual Usuario? GerenteAprovador { get; set; }

    public virtual ICollection<Comissao> Comissoes { get; set; } = new List<Comissao>();
}
