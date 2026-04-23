namespace ImovelStand.Application.Abstractions;

/// <summary>
/// Abstração do provedor de WhatsApp Business API oficial (Meta Cloud API
/// ou BSP como 360dialog/Twilio). Distinta de <see cref="INotificador"/>
/// (que usa Z-API para notificações transacionais simples).
/// </summary>
public interface IWhatsAppOficialProvider
{
    /// <summary>
    /// Envia mensagem via template (único formato permitido fora da janela
    /// de 24h de sessão).
    /// </summary>
    Task<EnvioResultado> EnviarTemplateAsync(EnvioTemplateRequest request, CancellationToken ct = default);

    /// <summary>
    /// Envia texto livre (somente dentro da janela de sessão 24h após
    /// cliente escrever).
    /// </summary>
    Task<EnvioResultado> EnviarTextoAsync(EnvioTextoRequest request, CancellationToken ct = default);
}

public class EnvioTemplateRequest
{
    public string NumeroDestino { get; set; } = string.Empty;
    public string NumeroRemetente { get; set; } = string.Empty;
    public string NomeTemplate { get; set; } = string.Empty;
    public string Idioma { get; set; } = "pt_BR";
    public List<string> Variaveis { get; set; } = new();
    public string? MediaUrl { get; set; }
}

public class EnvioTextoRequest
{
    public string NumeroDestino { get; set; } = string.Empty;
    public string NumeroRemetente { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
}

public class EnvioResultado
{
    public bool Sucesso { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? MensagemErro { get; set; }
}

/// <summary>
/// Configuração do módulo WhatsApp oficial.
/// </summary>
public class WhatsAppOficialOptions
{
    public const string SectionName = "WhatsAppOficial";

    /// <summary>
    /// "meta-cloud" (padrão), "360dialog", "twilio", "stub" (dev).
    /// </summary>
    public string Provider { get; set; } = "stub";

    /// <summary>
    /// Access token do Meta Business (permanent) ou BSP.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Phone Number ID do Meta (não é o número em si, é o ID interno).
    /// </summary>
    public string PhoneNumberId { get; set; } = string.Empty;

    /// <summary>
    /// Versão da Graph API (ex: v21.0).
    /// </summary>
    public string ApiVersion { get; set; } = "v21.0";

    /// <summary>
    /// Verify token do webhook (usado em GET de validação do Meta).
    /// </summary>
    public string WebhookVerifyToken { get; set; } = string.Empty;

    /// <summary>
    /// Se true, envios só logam e retornam sucesso (modo dev).
    /// </summary>
    public bool ModoStub { get; set; } = true;
}
