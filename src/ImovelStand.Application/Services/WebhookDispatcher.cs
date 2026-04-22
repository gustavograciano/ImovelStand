using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ImovelStand.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ImovelStand.Application.Services;

public class WebhookDispatcher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookDispatcher> _logger;

    public WebhookDispatcher(IHttpClientFactory httpClientFactory, ILogger<WebhookDispatcher> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Dispara webhook assíncrono: retry exponencial (1s, 2s, 4s) se falhar.
    /// Se configurado Secret, assina o payload com HMAC-SHA256 em header X-Signature.
    /// </summary>
    public async Task<bool> DispatchAsync(WebhookSubscription sub, string evento, object payload, CancellationToken cancellationToken = default)
    {
        if (!sub.Ativo) return false;

        var body = new
        {
            evento,
            dispatchedAt = DateTime.UtcNow,
            data = payload
        };
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        if (!string.IsNullOrWhiteSpace(sub.Secret))
        {
            var signature = ComputeSignature(json, sub.Secret);
            content.Headers.Add("X-Signature", signature);
        }
        content.Headers.Add("X-Event", evento);

        var http = _httpClientFactory.CreateClient();
        http.Timeout = TimeSpan.FromSeconds(15);

        TimeSpan[] backoff = { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4) };

        for (var tentativa = 0; tentativa < backoff.Length; tentativa++)
        {
            try
            {
                var response = await http.PostAsync(sub.Url, content, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Webhook {Url} disparado ({Evento}) na tentativa {Tent}", sub.Url, evento, tentativa + 1);
                    return true;
                }
                _logger.LogWarning("Webhook {Url} retornou {Status} (tent {Tent})", sub.Url, response.StatusCode, tentativa + 1);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao disparar webhook {Url} (tent {Tent})", sub.Url, tentativa + 1);
            }

            if (tentativa < backoff.Length - 1)
                await Task.Delay(backoff[tentativa], cancellationToken);
        }

        return false;
    }

    public static string ComputeSignature(string payload, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var bytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(bytes);
        return $"sha256={Convert.ToHexString(hash).ToLowerInvariant()}";
    }
}
