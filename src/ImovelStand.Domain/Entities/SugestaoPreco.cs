using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

/// <summary>
/// Sugestão de ajuste de preço gerada pelo motor de precificação dinâmica.
/// Histórico completo para auditoria e feedback loop (aceitou/rejeitou).
/// </summary>
public class SugestaoPreco : ITenantEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public int ApartamentoId { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal PrecoAtual { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal PrecoSugerido { get; set; }

    /// <summary>
    /// Variação percentual sugerida (positiva = aumento, negativa = desconto).
    /// </summary>
    [Column(TypeName = "decimal(6,2)")]
    public decimal VariacaoPct { get; set; }

    /// <summary>
    /// Motivo da sugestão (ex: "vende-40pct-mais-rapido", "encalhado",
    /// "mercado-subiu", "abaixo-da-media").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Explicação em linguagem natural para o gerente comercial.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Justificativa { get; set; } = string.Empty;

    /// <summary>
    /// Velocidade de venda observada (unidades/semana).
    /// </summary>
    [Column(TypeName = "decimal(6,2)")]
    public decimal VelocidadeVendaSemanal { get; set; }

    /// <summary>
    /// Velocidade média do mercado para mesma tipologia/região.
    /// </summary>
    [Column(TypeName = "decimal(6,2)")]
    public decimal VelocidadeMercado { get; set; }

    /// <summary>
    /// Confiança da sugestão 0-100.
    /// </summary>
    public int Confianca { get; set; }

    /// <summary>
    /// Status: 'pendente' | 'aceita' | 'rejeitada' | 'expirada'.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "pendente";

    public int? AceitaPorUsuarioId { get; set; }
    public DateTime? RespondidaEm { get; set; }

    [MaxLength(500)]
    public string? MotivoRejeicao { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ApartamentoId))]
    public virtual Apartamento Apartamento { get; set; } = null!;

    [ForeignKey(nameof(AceitaPorUsuarioId))]
    public virtual Usuario? AceitaPorUsuario { get; set; }
}
