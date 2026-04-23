using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

public enum DirecaoWhatsApp
{
    Enviada = 0,
    Recebida = 1
}

public enum StatusMensagemWhatsApp
{
    /// <summary>Mensagem criada localmente, ainda não enviada ao provedor.</summary>
    Pendente = 0,
    /// <summary>Aceita pelo provedor (Meta) para envio.</summary>
    Aceita = 1,
    /// <summary>Confirmada como entregue ao dispositivo do destinatário.</summary>
    Entregue = 2,
    /// <summary>Lida pelo destinatário (double-check azul).</summary>
    Lida = 3,
    /// <summary>Falhou no envio — ver MensagemErro.</summary>
    Falhou = 99
}

/// <summary>
/// Persistência de mensagens WhatsApp via API oficial do Meta.
/// Cada mensagem vira também uma HistoricoInteracao no perfil do cliente.
/// </summary>
public class WhatsAppMensagem : ITenantEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    public int? ClienteId { get; set; }

    public int? UsuarioId { get; set; }

    /// <summary>
    /// Número do destinatário/remetente no formato E.164 (+5511999998888).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string TelefoneContato { get; set; } = string.Empty;

    /// <summary>
    /// Número da incorporadora que enviou ou recebeu (E.164).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string NumeroEmpresa { get; set; } = string.Empty;

    [Required]
    public DirecaoWhatsApp Direcao { get; set; }

    [Required]
    public StatusMensagemWhatsApp Status { get; set; } = StatusMensagemWhatsApp.Pendente;

    /// <summary>
    /// Template usado (apenas em Enviada com template). Null = texto livre
    /// dentro de janela de sessão 24h.
    /// </summary>
    public int? TemplateId { get; set; }

    /// <summary>
    /// Variáveis usadas no template, em JSON.
    /// </summary>
    [MaxLength(2000)]
    public string? VariaveisJson { get; set; }

    /// <summary>
    /// Corpo da mensagem (pós-substituição de variáveis).
    /// </summary>
    [MaxLength(4000)]
    public string Conteudo { get; set; } = string.Empty;

    /// <summary>
    /// URL de mídia anexa (PDF de proposta, foto, etc).
    /// </summary>
    [MaxLength(500)]
    public string? MediaUrl { get; set; }

    /// <summary>
    /// ID da mensagem no provedor (Meta), usado para correlacionar webhooks.
    /// </summary>
    [MaxLength(100)]
    public string? ProviderMessageId { get; set; }

    [MaxLength(500)]
    public string? MensagemErro { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EnviadaEm { get; set; }
    public DateTime? EntregueEm { get; set; }
    public DateTime? LidaEm { get; set; }

    [ForeignKey(nameof(ClienteId))]
    public virtual Cliente? Cliente { get; set; }

    [ForeignKey(nameof(TemplateId))]
    public virtual WhatsAppTemplate? Template { get; set; }
}
