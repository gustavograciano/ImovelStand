using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

/// <summary>
/// Audit log de toda chamada ao LLM. Usado para:
/// - Cost tracking por tenant
/// - Debug de prompts problemáticos
/// - Rate limiting baseado em histórico
/// - Regression testing (eval framework)
/// </summary>
public class IAInteracao : ITenantEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    public int? UsuarioId { get; set; }

    /// <summary>
    /// Operação executada (ex: "briefing-cliente", "extrair-proposta", "proximas-acoes").
    /// Usado para agrupar métricas.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string Operacao { get; set; } = string.Empty;

    /// <summary>
    /// Versão do prompt usada (ex: "v1", "v2"). Permite A/B e rollback.
    /// </summary>
    [Required]
    [MaxLength(16)]
    public string PromptVersao { get; set; } = "v1";

    /// <summary>
    /// Modelo usado (ex: "claude-sonnet-4-5", "claude-haiku").
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string Modelo { get; set; } = string.Empty;

    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }

    [Column(TypeName = "decimal(10,6)")]
    public decimal CustoUsd { get; set; }

    /// <summary>
    /// Duração da chamada em milissegundos.
    /// </summary>
    public int DuracaoMs { get; set; }

    public bool Sucesso { get; set; } = true;

    [MaxLength(500)]
    public string? MensagemErro { get; set; }

    /// <summary>
    /// Hash SHA-256 do input — usado como cache key. Não armazenamos o input
    /// completo para evitar bloat do banco e vazamento de dados sensíveis.
    /// </summary>
    [MaxLength(64)]
    public string? InputHash { get; set; }

    /// <summary>
    /// Indica se a resposta veio do cache (não custou tokens).
    /// </summary>
    public bool DoCache { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UsuarioId))]
    public virtual Usuario? Usuario { get; set; }
}
