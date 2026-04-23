using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ImovelStand.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImovelStand.Infrastructure.WhatsApp;

/// <summary>
/// Implementação de IWhatsAppOficialProvider usando a Meta Cloud API
/// diretamente (sem BSP). Em produção pode-se trocar por 360dialog/Twilio
/// sem mexer em callers.
///
/// Docs: https://developers.facebook.com/docs/whatsapp/cloud-api
/// </summary>
public class MetaCloudProvider : IWhatsAppOficialProvider
{
    private readonly WhatsAppOficialOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MetaCloudProvider> _logger;

    public MetaCloudProvider(
        IOptions<WhatsAppOficialOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<MetaCloudProvider> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<EnvioResultado> EnviarTemplateAsync(EnvioTemplateRequest request, CancellationToken ct = default)
    {
        if (_options.ModoStub)
        {
            _logger.LogInformation("[Meta Stub] Template {Nome} → {Destino} vars={Vars}",
                request.NomeTemplate, request.NumeroDestino, string.Join(",", request.Variaveis));
            return new EnvioResultado { Sucesso = true, ProviderMessageId = $"stub-{Guid.NewGuid():N}" };
        }

        if (string.IsNullOrWhiteSpace(_options.AccessToken) || string.IsNullOrWhiteSpace(_options.PhoneNumberId))
        {
            return new EnvioResultado { Sucesso = false, MensagemErro = "AccessToken ou PhoneNumberId nao configurados." };
        }

        var parametros = request.Variaveis
            .Select(v => new MetaParameter { Type = "text", Text = v })
            .ToList();

        var body = new MetaMessageRequest
        {
            MessagingProduct = "whatsapp",
            To = NormalizaNumero(request.NumeroDestino),
            Type = "template",
            Template = new MetaTemplate
            {
                Name = request.NomeTemplate,
                Language = new MetaLanguage { Code = request.Idioma },
                Components = parametros.Count > 0
                    ? new List<MetaComponent> { new() { Type = "body", Parameters = parametros } }
                    : null
            }
        };

        return await ChamarApiAsync(body, ct);
    }

    public async Task<EnvioResultado> EnviarTextoAsync(EnvioTextoRequest request, CancellationToken ct = default)
    {
        if (_options.ModoStub)
        {
            _logger.LogInformation("[Meta Stub] Texto livre → {Destino}: {Texto}",
                request.NumeroDestino, Truncate(request.Texto, 80));
            return new EnvioResultado { Sucesso = true, ProviderMessageId = $"stub-{Guid.NewGuid():N}" };
        }

        var body = new MetaMessageRequest
        {
            MessagingProduct = "whatsapp",
            To = NormalizaNumero(request.NumeroDestino),
            Type = "text",
            Text = new MetaText { Body = request.Texto, PreviewUrl = true }
        };

        return await ChamarApiAsync(body, ct);
    }

    private async Task<EnvioResultado> ChamarApiAsync(MetaMessageRequest body, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("meta-whatsapp");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.AccessToken}");

            var url = $"https://graph.facebook.com/{_options.ApiVersion}/{_options.PhoneNumberId}/messages";
            var response = await client.PostAsJsonAsync(url, body, ct);
            var content = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Meta API erro {Status}: {Body}", response.StatusCode, Truncate(content, 500));
                return new EnvioResultado
                {
                    Sucesso = false,
                    MensagemErro = $"{(int)response.StatusCode}: {Truncate(content, 200)}"
                };
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<MetaMessageResponse>(content);
                var id = parsed?.Messages?.FirstOrDefault()?.Id;
                return new EnvioResultado { Sucesso = true, ProviderMessageId = id };
            }
            catch (JsonException)
            {
                return new EnvioResultado { Sucesso = true, ProviderMessageId = null };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Meta API: exceção no envio.");
            return new EnvioResultado { Sucesso = false, MensagemErro = ex.Message };
        }
    }

    private static string NormalizaNumero(string numero)
    {
        var n = new string(numero.Where(c => char.IsDigit(c)).ToArray());
        // Se não tem código de país, assume +55 (Brasil)
        if (n.Length == 11 || n.Length == 10) n = "55" + n;
        return n;
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];

    // ========== DTOs Meta ==========

    private class MetaMessageRequest
    {
        [JsonPropertyName("messaging_product")] public string MessagingProduct { get; set; } = string.Empty;
        [JsonPropertyName("to")] public string To { get; set; } = string.Empty;
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
        [JsonPropertyName("template")] public MetaTemplate? Template { get; set; }
        [JsonPropertyName("text")] public MetaText? Text { get; set; }
    }

    private class MetaTemplate
    {
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("language")] public MetaLanguage Language { get; set; } = new();
        [JsonPropertyName("components")] public List<MetaComponent>? Components { get; set; }
    }

    private class MetaLanguage
    {
        [JsonPropertyName("code")] public string Code { get; set; } = "pt_BR";
    }

    private class MetaComponent
    {
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
        [JsonPropertyName("parameters")] public List<MetaParameter> Parameters { get; set; } = new();
    }

    private class MetaParameter
    {
        [JsonPropertyName("type")] public string Type { get; set; } = "text";
        [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;
    }

    private class MetaText
    {
        [JsonPropertyName("body")] public string Body { get; set; } = string.Empty;
        [JsonPropertyName("preview_url")] public bool PreviewUrl { get; set; }
    }

    private class MetaMessageResponse
    {
        [JsonPropertyName("messages")] public List<MetaMessageId>? Messages { get; set; }
    }

    private class MetaMessageId
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    }
}
