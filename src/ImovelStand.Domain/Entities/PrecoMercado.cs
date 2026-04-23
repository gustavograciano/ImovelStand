using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImovelStand.Domain.Entities;

/// <summary>
/// Snapshot de preço médio do mercado imobiliário para uma combinação
/// cidade+bairro+qtd quartos. Alimentado por data source externo
/// (FIPE-ZAP, ZAP Imóveis API, ou parceria de dados cooperativos).
///
/// Dados são globais (não ITenantEntity) — todos os tenants usam o
/// mesmo índice de mercado.
/// </summary>
public class PrecoMercado
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Cidade { get; set; } = string.Empty;

    [Required]
    [MaxLength(2)]
    public string Uf { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Bairro { get; set; }

    /// <summary>
    /// Qtd de quartos (1, 2, 3, 4+) para granularidade mínima.
    /// </summary>
    public int Quartos { get; set; }

    /// <summary>
    /// Preço médio por m² naquele segmento.
    /// </summary>
    [Column(TypeName = "decimal(12,2)")]
    public decimal PrecoMedioM2 { get; set; }

    /// <summary>
    /// Desvio padrão (indica volatilidade). Usado para determinar
    /// intervalo de confiança da sugestão.
    /// </summary>
    [Column(TypeName = "decimal(12,2)")]
    public decimal DesvioPadraoM2 { get; set; }

    /// <summary>
    /// Quantidade de amostras no cálculo. Se < 20, confiança baixa,
    /// sugestão deve ser desabilitada.
    /// </summary>
    public int QtdAmostras { get; set; }

    /// <summary>
    /// Fonte dos dados (ex: "fipe-zap", "zap-imoveis", "manual").
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Fonte { get; set; } = "manual";

    /// <summary>
    /// Data de referência do snapshot (normalmente início do mês).
    /// </summary>
    public DateTime DataReferencia { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
