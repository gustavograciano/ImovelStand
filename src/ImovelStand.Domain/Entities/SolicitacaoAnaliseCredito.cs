using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

public enum StatusAnaliseCredito
{
    /// <summary>Solicitação criada, aguardando cliente autorizar Open Finance.</summary>
    Pendente = 0,
    /// <summary>Cliente autorizou, iniciadora puxando dados.</summary>
    EmProcessamento = 1,
    /// <summary>Análise concluída — dados disponíveis.</summary>
    Concluida = 2,
    /// <summary>Cliente revogou consentimento ou expiracao.</summary>
    Revogada = 3,
    /// <summary>Falha técnica ao coletar dados.</summary>
    Falhou = 99
}

/// <summary>
/// Solicitação de análise de crédito via Open Finance. Gerada pelo corretor,
/// cliente autoriza via link único, sistema puxa extratos e calcula
/// capacidade de pagamento real.
///
/// Retenção de dados: LGPD + Bacen exigem max 12 meses, depois expurgo.
/// </summary>
public class SolicitacaoAnaliseCredito : ITenantEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public int ClienteId { get; set; }

    public int? SolicitadoPorUsuarioId { get; set; }

    [Required]
    public StatusAnaliseCredito Status { get; set; } = StatusAnaliseCredito.Pendente;

    /// <summary>
    /// Token único do link de autorização (usado na URL enviada ao cliente).
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Provedor (iniciadora) — "pluggy", "belvo", "klavi", "stub".
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Provedor { get; set; } = "stub";

    /// <summary>
    /// ID do "Item" no Pluggy (= conexão do cliente com um banco).
    /// </summary>
    [MaxLength(100)]
    public string? ProviderItemId { get; set; }

    /// <summary>
    /// Renda mensal líquida média calculada a partir de extratos (últimos 6m).
    /// </summary>
    [Column(TypeName = "decimal(12,2)")]
    public decimal? RendaMediaComprovada { get; set; }

    /// <summary>
    /// Desvio padrão da renda (estabilidade — quanto menor, mais estável).
    /// </summary>
    [Column(TypeName = "decimal(12,2)")]
    public decimal? VolatilidadeRenda { get; set; }

    /// <summary>
    /// Soma de dividas recorrentes mensais identificadas.
    /// </summary>
    [Column(TypeName = "decimal(12,2)")]
    public decimal? DividasRecorrentes { get; set; }

    /// <summary>
    /// Capacidade de pagamento calculada (renda_liquida*0.3 - dividas).
    /// </summary>
    [Column(TypeName = "decimal(12,2)")]
    public decimal? CapacidadePagamento { get; set; }

    /// <summary>
    /// Score proprio 0-1000 (combina renda, estabilidade, dividas).
    /// Não substitui Serasa/SPC — complementa com dados bancários reais.
    /// </summary>
    public int? Score { get; set; }

    /// <summary>
    /// Alertas identificados (apostas, parcelamentos excessivos, etc).
    /// </summary>
    [MaxLength(1000)]
    public string? AlertasJson { get; set; }

    /// <summary>
    /// Consentimento LGPD específico para este processamento.
    /// </summary>
    public bool ConsentimentoLgpd { get; set; }

    public DateTime? ConsentimentoLgpdEm { get; set; }

    /// <summary>
    /// Data de expiração. Após isso, dados coletados devem ser expurgados.
    /// </summary>
    public DateTime ExpiraEm { get; set; } = DateTime.UtcNow.AddMonths(12);

    public DateTime? ConcluidaEm { get; set; }

    [MaxLength(500)]
    public string? MensagemErro { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ClienteId))]
    public virtual Cliente Cliente { get; set; } = null!;

    [ForeignKey(nameof(SolicitadoPorUsuarioId))]
    public virtual Usuario? SolicitadoPorUsuario { get; set; }
}
