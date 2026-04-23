using System.ComponentModel.DataAnnotations;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

/// <summary>
/// Template de WhatsApp aprovado pelo Meta (obrigatório para envios
/// fora da janela de 24h de sessão). Cada tenant cadastra seus próprios.
/// </summary>
public class WhatsAppTemplate : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// Nome do template registrado no Meta (ex: "confirmacao_visita_v1").
    /// Case-sensitive, lowercase, snake_case.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Idioma do template (ex: "pt_BR").
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Idioma { get; set; } = "pt_BR";

    /// <summary>
    /// Categoria Meta (UTILITY, MARKETING, AUTHENTICATION).
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Categoria { get; set; } = "UTILITY";

    /// <summary>
    /// Corpo do template com placeholders {{1}}, {{2}}, etc.
    /// Duplicado aqui para preview local — verdade absoluta é no Meta.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Corpo { get; set; } = string.Empty;

    /// <summary>
    /// Quantidade esperada de variáveis.
    /// </summary>
    public int QtdVariaveis { get; set; }

    /// <summary>
    /// Label amigável para o corretor escolher (ex: "Confirmação de visita").
    /// </summary>
    [MaxLength(100)]
    public string? Descricao { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
