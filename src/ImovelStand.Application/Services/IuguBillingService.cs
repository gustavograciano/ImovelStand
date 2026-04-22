using System.Net.Http;
using System.Net.Http.Json;
using ImovelStand.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImovelStand.Application.Services;

public class IuguOptions
{
    public string ApiUrl { get; set; } = "https://api.iugu.com/v1";
    public string? ApiToken { get; set; }
    public int TrialDias { get; set; } = 14;
}

/// <summary>
/// Stub de integração com Iugu. Em dev sem token, retorna IDs fake para
/// permitir desenvolvimento end-to-end sem depender de conta sandbox.
/// Em prod, a interface é a mesma — só muda o ApiToken.
/// </summary>
public class IuguBillingService
{
    private readonly IuguOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IuguBillingService> _logger;

    public IuguBillingService(
        IOptions<IuguOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<IuguBillingService> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiToken);

    public async Task<string> CreateCustomerAsync(Tenant tenant, string email, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogInformation("Iugu não configurada. Criando customer fake para tenant {Id}", tenant.Id);
            return $"fake-iugu-cust-{tenant.Id}";
        }

        var http = _httpClientFactory.CreateClient();
        var response = await http.PostAsJsonAsync(
            $"{_options.ApiUrl}/customers?api_token={_options.ApiToken}",
            new { email, name = tenant.Nome, cpf_cnpj = tenant.Cnpj },
            cancellationToken);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<IuguCustomerResponse>(cancellationToken: cancellationToken);
        return body?.Id ?? throw new InvalidOperationException("Iugu não retornou Id de customer.");
    }

    public async Task<string> CreateSubscriptionAsync(string customerId, Plano plano, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogInformation("Iugu não configurada. Subscription fake para customer {Id}", customerId);
            return $"fake-iugu-sub-{customerId}-{plano.Id}";
        }

        var http = _httpClientFactory.CreateClient();
        var response = await http.PostAsJsonAsync(
            $"{_options.ApiUrl}/subscriptions?api_token={_options.ApiToken}",
            new
            {
                customer_id = customerId,
                plan_identifier = plano.Nome.ToLowerInvariant(),
                expires_at = DateTime.UtcNow.AddDays(_options.TrialDias).ToString("yyyy-MM-dd")
            },
            cancellationToken);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<IuguSubscriptionResponse>(cancellationToken: cancellationToken);
        return body?.Id ?? throw new InvalidOperationException("Iugu não retornou Id de subscription.");
    }

    public async Task CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured) return;
        var http = _httpClientFactory.CreateClient();
        var response = await http.PostAsync(
            $"{_options.ApiUrl}/subscriptions/{subscriptionId}/suspend?api_token={_options.ApiToken}",
            content: null,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private sealed record IuguCustomerResponse(string Id, string Email);
    private sealed record IuguSubscriptionResponse(string Id, string CustomerId);
}
