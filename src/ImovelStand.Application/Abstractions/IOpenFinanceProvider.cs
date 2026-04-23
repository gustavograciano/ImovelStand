namespace ImovelStand.Application.Abstractions;

/// <summary>
/// Abstração sobre iniciadoras Open Finance (Pluggy, Belvo, Klavi, etc).
/// MVP usa Pluggy; Belvo como segunda opção. Implementação stub
/// retorna dados fake para desenvolvimento sem credenciais.
/// </summary>
public interface IOpenFinanceProvider
{
    /// <summary>
    /// Cria um Connect Token para o cliente autorizar acesso aos bancos.
    /// Retorna URL para o widget de conexão (redirect bancário).
    /// </summary>
    Task<ConnectLinkResult> CriarConnectLinkAsync(string clienteIdentifier, CancellationToken ct = default);

    /// <summary>
    /// Baixa extratos e dados consolidados do Item (conexão) após autorização.
    /// </summary>
    Task<OpenFinanceDadosBrutos> BuscarDadosAsync(string providerItemId, CancellationToken ct = default);

    /// <summary>
    /// Nome do provedor (log/audit).
    /// </summary>
    string Nome { get; }
}

public class ConnectLinkResult
{
    public bool Sucesso { get; set; }
    public string? ConnectUrl { get; set; }
    public string? ConnectToken { get; set; }
    public string? MensagemErro { get; set; }
}

public class OpenFinanceDadosBrutos
{
    public bool Sucesso { get; set; }
    public string? MensagemErro { get; set; }

    /// <summary>
    /// Transações dos últimos 6 meses (créditos e débitos).
    /// </summary>
    public List<TransacaoBancaria> Transacoes { get; set; } = new();

    /// <summary>
    /// Nome do banco e titular (para referência).
    /// </summary>
    public string? NomeBanco { get; set; }
    public string? NomeTitular { get; set; }
}

public class TransacaoBancaria
{
    public DateTime Data { get; set; }
    public decimal Valor { get; set; } // positivo = crédito, negativo = débito
    public string Descricao { get; set; } = string.Empty;
    public string? Categoria { get; set; } // "salario", "aluguel", "financiamento", etc
}

public class OpenFinanceOptions
{
    public const string SectionName = "OpenFinance";

    public string Provider { get; set; } = "stub"; // "pluggy", "belvo", "stub"
    public string ApiUrl { get; set; } = "https://api.pluggy.ai";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string? RedirectUrl { get; set; }
}
