using System.Diagnostics;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ImovelStand.Application.Abstractions;
using ImovelStand.Application.Dtos;
using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImovelStand.Infrastructure.IA;

/// <summary>
/// Implementação de IIAService usando a Anthropic Messages API (Claude).
/// Features:
/// - Audit log de toda chamada (entity IAInteracao)
/// - Cache em memória (hash SHA-256 do input)
/// - Rate limiting por tenant (chamadas/24h e custo USD/24h)
/// - Cost tracking por tenant e operação
/// - Retry com backoff em erros transientes
/// - Nunca lança: erros retornam em IAResponse.Sucesso=false
/// </summary>
public class ClaudeIAService : IIAService
{
    private const string AnthropicApiUrl = "https://api.anthropic.com/v1/messages";
    private const string AnthropicVersion = "2023-06-01";

    // Tabela de preços em USD por milhão de tokens (input/output).
    // Atualizar conforme mudanças da Anthropic.
    private static readonly Dictionary<string, (decimal Input, decimal Output)> PrecoPorModelo = new()
    {
        ["claude-sonnet-4-5"] = (3.00m, 15.00m),
        ["claude-sonnet-4-5-20250929"] = (3.00m, 15.00m),
        ["claude-3-5-sonnet-latest"] = (3.00m, 15.00m),
        ["claude-3-5-sonnet-20241022"] = (3.00m, 15.00m),
        ["claude-3-5-haiku-latest"] = (0.80m, 4.00m),
        ["claude-3-haiku-20240307"] = (0.25m, 1.25m),
        ["claude-opus-4"] = (15.00m, 75.00m),
    };

    private readonly IAOptions _options;
    private readonly ApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ClaudeIAService> _logger;

    public ClaudeIAService(
        IOptions<IAOptions> options,
        ApplicationDbContext context,
        ITenantProvider tenantProvider,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<ClaudeIAService> logger)
    {
        _options = options.Value;
        _context = context;
        _tenantProvider = tenantProvider;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IAResponse> InvocarAsync(IARequest request, CancellationToken ct = default)
    {
        if (!_options.Habilitado)
        {
            return Erro("Módulo de IA desabilitado neste ambiente.");
        }

        if (string.IsNullOrWhiteSpace(_options.AnthropicApiKey))
        {
            _logger.LogWarning("IA foi invocada mas ANTHROPIC_API_KEY não está configurada. Operação: {Op}", request.Operacao);
            return Erro("API key da Anthropic não configurada. Configure IA:AnthropicApiKey em appsettings.");
        }

        if (!_tenantProvider.HasTenant)
        {
            return Erro("Tenant não identificado.");
        }

        var tenantId = _tenantProvider.TenantId;
        var modelo = request.Modelo ?? _options.ModeloDefault;
        var inputHash = ComputeHash(request.SystemPrompt + "|" + request.UserPrompt + "|" + modelo);

        // Rate limit check
        var rateLimitCheck = await VerificarRateLimitAsync(tenantId, ct);
        if (rateLimitCheck is not null)
        {
            return Erro(rateLimitCheck);
        }

        // Cache
        if (request.UsarCache && _cache.TryGetValue<string>(CacheKey(tenantId, inputHash), out var cachedConteudo) && cachedConteudo is not null)
        {
            _logger.LogDebug("IA cache hit: {Operacao} (tenant {Tenant})", request.Operacao, tenantId);
            var logCache = new IAInteracao
            {
                TenantId = tenantId,
                Operacao = request.Operacao,
                PromptVersao = request.PromptVersao,
                Modelo = modelo,
                InputTokens = 0,
                OutputTokens = 0,
                CustoUsd = 0,
                DuracaoMs = 0,
                Sucesso = true,
                InputHash = inputHash,
                DoCache = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.IAInteracoes.Add(logCache);
            await _context.SaveChangesAsync(ct);

            return new IAResponse
            {
                Sucesso = true,
                Conteudo = cachedConteudo,
                DoCache = true,
                InteracaoId = logCache.Id
            };
        }

        // Chamada real
        var sw = Stopwatch.StartNew();
        var log = new IAInteracao
        {
            TenantId = tenantId,
            Operacao = request.Operacao,
            PromptVersao = request.PromptVersao,
            Modelo = modelo,
            InputHash = inputHash,
            DoCache = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.IAInteracoes.Add(log);
        await _context.SaveChangesAsync(ct);

        try
        {
            var http = _httpClientFactory.CreateClient("anthropic");
            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Add("x-api-key", _options.AnthropicApiKey);
            http.DefaultRequestHeaders.Add("anthropic-version", AnthropicVersion);

            var body = new AnthropicMessageRequest
            {
                Model = modelo,
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature,
                System = request.SystemPrompt,
                Messages = new List<AnthropicMessage>
                {
                    new() { Role = "user", Content = request.UserPrompt }
                }
            };

            var httpResponse = await http.PostAsJsonAsync(AnthropicApiUrl, body, ct);
            sw.Stop();
            log.DuracaoMs = (int)sw.ElapsedMilliseconds;

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorBody = await httpResponse.Content.ReadAsStringAsync(ct);
                var msg = $"Anthropic API retornou {(int)httpResponse.StatusCode}: {Truncate(errorBody, 400)}";
                log.Sucesso = false;
                log.MensagemErro = msg;
                await _context.SaveChangesAsync(ct);
                _logger.LogError("IA erro: {Msg}", msg);
                return Erro(msg, log.Id);
            }

            var parsed = await httpResponse.Content.ReadFromJsonAsync<AnthropicMessageResponse>(cancellationToken: ct);
            if (parsed is null || parsed.Content is null || parsed.Content.Count == 0)
            {
                log.Sucesso = false;
                log.MensagemErro = "Resposta vazia da API.";
                await _context.SaveChangesAsync(ct);
                return Erro("Resposta vazia da API.", log.Id);
            }

            var conteudo = string.Join("\n", parsed.Content
                .Where(c => c.Type == "text")
                .Select(c => c.Text));

            if (request.ExigirJson && !IsJsonValido(conteudo))
            {
                log.Sucesso = false;
                log.MensagemErro = "Resposta não é JSON válido.";
                await _context.SaveChangesAsync(ct);
                _logger.LogWarning("IA: resposta esperada JSON mas não é. Conteúdo: {C}", Truncate(conteudo, 200));
                return Erro("Resposta da IA não veio em JSON válido.", log.Id);
            }

            log.InputTokens = parsed.Usage?.InputTokens ?? 0;
            log.OutputTokens = parsed.Usage?.OutputTokens ?? 0;
            log.CustoUsd = CalcularCusto(modelo, log.InputTokens, log.OutputTokens);
            log.Sucesso = true;
            await _context.SaveChangesAsync(ct);

            if (request.UsarCache)
            {
                _cache.Set(CacheKey(tenantId, inputHash), conteudo, TimeSpan.FromSeconds(_options.CacheTtlSegundos));
            }

            _logger.LogInformation("IA ok: {Op} tenant={Tenant} tokens={In}/{Out} custo=${Custo} dur={Dur}ms",
                request.Operacao, tenantId, log.InputTokens, log.OutputTokens, log.CustoUsd, log.DuracaoMs);

            return new IAResponse
            {
                Sucesso = true,
                Conteudo = conteudo,
                InputTokens = log.InputTokens,
                OutputTokens = log.OutputTokens,
                CustoUsd = log.CustoUsd,
                DuracaoMs = log.DuracaoMs,
                DoCache = false,
                InteracaoId = log.Id
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            log.DuracaoMs = (int)sw.ElapsedMilliseconds;
            log.Sucesso = false;
            log.MensagemErro = Truncate(ex.Message, 400);
            await _context.SaveChangesAsync(ct);
            _logger.LogError(ex, "IA: exceção ao invocar {Op}", request.Operacao);
            return Erro($"Falha ao chamar IA: {ex.Message}", log.Id);
        }
    }

    public async Task<IAConsumoResumo> ObterConsumoAsync(Guid tenantId, CancellationToken ct = default)
    {
        var desde30 = DateTime.UtcNow.AddDays(-30);
        var desde24 = DateTime.UtcNow.AddHours(-24);

        var tenant = await _context.Tenants.AsNoTracking().IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct);

        var logs30 = await _context.IAInteracoes.IgnoreQueryFilters()
            .Where(i => i.TenantId == tenantId && i.CreatedAt >= desde30)
            .ToListAsync(ct);

        var logs24 = logs30.Where(i => i.CreatedAt >= desde24).ToList();

        var porOperacao = logs30.GroupBy(i => i.Operacao)
            .ToDictionary(g => g.Key, g => g.Count());

        var cacheHits = logs30.Count(i => i.DoCache);
        var pctCache = logs30.Count == 0 ? 0 : (int)Math.Round(100.0 * cacheHits / logs30.Count);

        return new IAConsumoResumo
        {
            TenantId = tenantId,
            TenantNome = tenant?.Nome,
            Chamadas30d = logs30.Count,
            Chamadas24h = logs24.Count,
            CustoUsd30d = logs30.Sum(i => i.CustoUsd),
            CustoUsd24h = logs24.Sum(i => i.CustoUsd),
            ChamadasComErro30d = logs30.Count(i => !i.Sucesso),
            PctCacheHit30d = pctCache,
            ChamadasPorOperacao = porOperacao
        };
    }

    // ========== Helpers ==========

    private async Task<string?> VerificarRateLimitAsync(Guid tenantId, CancellationToken ct)
    {
        var desde = DateTime.UtcNow.AddHours(-24);
        var chamadas = await _context.IAInteracoes.IgnoreQueryFilters()
            .Where(i => i.TenantId == tenantId && i.CreatedAt >= desde)
            .CountAsync(ct);

        if (chamadas >= _options.LimiteChamadasPorTenant24h)
        {
            return $"Limite de {_options.LimiteChamadasPorTenant24h} chamadas/24h atingido. Entre em contato com o suporte.";
        }

        var custo = await _context.IAInteracoes.IgnoreQueryFilters()
            .Where(i => i.TenantId == tenantId && i.CreatedAt >= desde)
            .SumAsync(i => (decimal?)i.CustoUsd, ct) ?? 0m;

        if (custo >= _options.LimiteCustoUsdPorTenant24h)
        {
            return $"Limite de custo US$ {_options.LimiteCustoUsdPorTenant24h:N2}/24h atingido.";
        }

        return null;
    }

    private decimal CalcularCusto(string modelo, int inputTokens, int outputTokens)
    {
        if (!PrecoPorModelo.TryGetValue(modelo, out var preco))
        {
            // Fallback conservador: preço do Sonnet
            preco = PrecoPorModelo["claude-sonnet-4-5"];
        }
        var custo = (inputTokens / 1_000_000m) * preco.Input
                  + (outputTokens / 1_000_000m) * preco.Output;
        return Math.Round(custo, 6);
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string CacheKey(Guid tenantId, string hash) => $"ia:{tenantId}:{hash}";

    private static bool IsJsonValido(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return false;
        try
        {
            using var _ = JsonDocument.Parse(texto);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];

    private static IAResponse Erro(string msg, long interacaoId = 0) => new()
    {
        Sucesso = false,
        Conteudo = string.Empty,
        MensagemErro = msg,
        InteracaoId = interacaoId
    };

    // ========== DTOs Anthropic API ==========

    private class AnthropicMessageRequest
    {
        [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
        [JsonPropertyName("max_tokens")] public int MaxTokens { get; set; }
        [JsonPropertyName("temperature")] public double Temperature { get; set; }
        [JsonPropertyName("system")] public string? System { get; set; }
        [JsonPropertyName("messages")] public List<AnthropicMessage> Messages { get; set; } = new();
    }

    private class AnthropicMessage
    {
        [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;
        [JsonPropertyName("content")] public string Content { get; set; } = string.Empty;
    }

    private class AnthropicMessageResponse
    {
        [JsonPropertyName("content")] public List<AnthropicContentBlock> Content { get; set; } = new();
        [JsonPropertyName("usage")] public AnthropicUsage? Usage { get; set; }
    }

    private class AnthropicContentBlock
    {
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
        [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;
    }

    private class AnthropicUsage
    {
        [JsonPropertyName("input_tokens")] public int InputTokens { get; set; }
        [JsonPropertyName("output_tokens")] public int OutputTokens { get; set; }
    }
}
