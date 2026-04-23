namespace ImovelStand.Application.Dtos;

public class IARequest
{
    /// <summary>
    /// Nome da operação (ex: "briefing-cliente"). Usado para métricas e
    /// seleção de prompt.
    /// </summary>
    public string Operacao { get; set; } = string.Empty;

    /// <summary>
    /// Versão do prompt a usar. Default é a versão atual.
    /// </summary>
    public string PromptVersao { get; set; } = "v1";

    /// <summary>
    /// Prompt do sistema (instruções permanentes).
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Prompt do usuário (contexto + pergunta).
    /// </summary>
    public string UserPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Modelo a usar. Default: claude-sonnet-4-5.
    /// </summary>
    public string? Modelo { get; set; }

    /// <summary>
    /// Max tokens de saída. Default: 1024.
    /// </summary>
    public int MaxTokens { get; set; } = 1024;

    /// <summary>
    /// Temperature (0-1). Default: 0.3 (mais determinístico).
    /// </summary>
    public double Temperature { get; set; } = 0.3;

    /// <summary>
    /// Se true, tenta cache (hash do input → resposta). TTL 1h.
    /// </summary>
    public bool UsarCache { get; set; } = true;

    /// <summary>
    /// Se true, a resposta DEVE ser JSON. Falha se não for parseável.
    /// </summary>
    public bool ExigirJson { get; set; }
}

public class IAResponse
{
    public bool Sucesso { get; set; }
    public string Conteudo { get; set; } = string.Empty;
    public string? MensagemErro { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public decimal CustoUsd { get; set; }
    public int DuracaoMs { get; set; }
    public bool DoCache { get; set; }
    public long InteracaoId { get; set; }
}

/// <summary>
/// Estatísticas de consumo de IA por tenant (dashboard admin).
/// </summary>
public class IAConsumoResumo
{
    public Guid TenantId { get; set; }
    public string? TenantNome { get; set; }
    public int Chamadas30d { get; set; }
    public int Chamadas24h { get; set; }
    public decimal CustoUsd30d { get; set; }
    public decimal CustoUsd24h { get; set; }
    public int ChamadasComErro30d { get; set; }
    public int PctCacheHit30d { get; set; }
    public Dictionary<string, int> ChamadasPorOperacao { get; set; } = new();
}
