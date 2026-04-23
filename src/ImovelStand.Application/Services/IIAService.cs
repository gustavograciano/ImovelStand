using ImovelStand.Application.Dtos;

namespace ImovelStand.Application.Services;

/// <summary>
/// Abstração do provedor de LLM. Implementação default: Claude (Anthropic).
/// Trocável para OpenAI/Gemini sem mexer em callers.
/// </summary>
public interface IIAService
{
    /// <summary>
    /// Executa uma chamada LLM com rate limiting, cache, audit e cost tracking.
    /// Nunca lança — erros vêm em IAResponse.Sucesso=false.
    /// </summary>
    Task<IAResponse> InvocarAsync(IARequest request, CancellationToken ct = default);

    /// <summary>
    /// Retorna estatísticas de consumo para dashboard admin.
    /// </summary>
    Task<IAConsumoResumo> ObterConsumoAsync(Guid tenantId, CancellationToken ct = default);
}

/// <summary>
/// Configuração do módulo de IA (lida de appsettings).
/// </summary>
public class IAOptions
{
    public const string SectionName = "IA";

    /// <summary>
    /// Chave da API Anthropic.
    /// </summary>
    public string AnthropicApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Modelo default.
    /// </summary>
    public string ModeloDefault { get; set; } = "claude-sonnet-4-5";

    /// <summary>
    /// Max chamadas por tenant nas últimas 24h. Protege contra loop/abuso.
    /// </summary>
    public int LimiteChamadasPorTenant24h { get; set; } = 5000;

    /// <summary>
    /// Custo máximo USD por tenant em 24h. Protege contra incidente de custo.
    /// </summary>
    public decimal LimiteCustoUsdPorTenant24h { get; set; } = 50m;

    /// <summary>
    /// TTL do cache em segundos (default 1h).
    /// </summary>
    public int CacheTtlSegundos { get; set; } = 3600;

    /// <summary>
    /// Se true, módulo IA está ativo. Se false, retorna erro amigável.
    /// </summary>
    public bool Habilitado { get; set; } = true;
}
