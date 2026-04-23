using ImovelStand.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace ImovelStand.Infrastructure.OpenFinance;

/// <summary>
/// Provedor stub para desenvolvimento sem credenciais Pluggy/Belvo reais.
/// Retorna dados fake determinísticos baseados no identifier do cliente,
/// permitindo testar o fluxo completo end-to-end.
/// </summary>
public class StubOpenFinanceProvider : IOpenFinanceProvider
{
    private readonly ILogger<StubOpenFinanceProvider> _logger;

    public StubOpenFinanceProvider(ILogger<StubOpenFinanceProvider> logger)
    {
        _logger = logger;
    }

    public string Nome => "stub";

    public Task<ConnectLinkResult> CriarConnectLinkAsync(string clienteIdentifier, CancellationToken ct = default)
    {
        _logger.LogInformation("[Stub OF] ConnectLink para {Cliente}", clienteIdentifier);
        var token = Guid.NewGuid().ToString("N");
        return Task.FromResult(new ConnectLinkResult
        {
            Sucesso = true,
            ConnectToken = token,
            ConnectUrl = $"https://connect.stub.local/auth?token={token}"
        });
    }

    public Task<OpenFinanceDadosBrutos> BuscarDadosAsync(string providerItemId, CancellationToken ct = default)
    {
        _logger.LogInformation("[Stub OF] Buscar dados para ItemId {Id}", providerItemId);

        // Gera 6 meses de transações fake determinísticas
        var transacoes = new List<TransacaoBancaria>();
        var hoje = DateTime.UtcNow.Date;
        var rnd = new Random(providerItemId.GetHashCode());

        for (var mes = 5; mes >= 0; mes--)
        {
            var primeiroDiaMes = hoje.AddMonths(-mes).AddDays(1 - hoje.Day);

            // Salario regular
            var salario = 8000m + rnd.Next(-500, 2000);
            transacoes.Add(new TransacaoBancaria
            {
                Data = primeiroDiaMes.AddDays(4),
                Valor = salario,
                Descricao = "SALARIO EMPRESA XYZ",
                Categoria = "salario"
            });

            // Aluguel saindo
            transacoes.Add(new TransacaoBancaria
            {
                Data = primeiroDiaMes.AddDays(5),
                Valor = -2200m,
                Descricao = "ALUGUEL IMOBILIARIA",
                Categoria = "aluguel"
            });

            // Cartão
            transacoes.Add(new TransacaoBancaria
            {
                Data = primeiroDiaMes.AddDays(10),
                Valor = -1500m - rnd.Next(-300, 800),
                Descricao = "FATURA CARTAO",
                Categoria = "cartao"
            });

            // Despesas variaveis
            for (var d = 0; d < 15; d++)
            {
                transacoes.Add(new TransacaoBancaria
                {
                    Data = primeiroDiaMes.AddDays(rnd.Next(1, 28)),
                    Valor = -(20m + rnd.Next(5, 200)),
                    Descricao = "COMPRA DIVERSOS",
                    Categoria = "outros"
                });
            }
        }

        return Task.FromResult(new OpenFinanceDadosBrutos
        {
            Sucesso = true,
            Transacoes = transacoes.OrderBy(t => t.Data).ToList(),
            NomeBanco = "Banco Fake",
            NomeTitular = "Cliente Demo"
        });
    }
}
