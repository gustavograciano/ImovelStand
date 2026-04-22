using ImovelStand.Application.Services;

namespace ImovelStand.Tests.Services;

public class WebhookDispatcherTests
{
    [Fact]
    public void ComputeSignature_ProduzHmacSha256()
    {
        const string payload = "{\"evento\":\"venda.criada\"}";
        const string secret = "meu-secret";

        var sig1 = WebhookDispatcher.ComputeSignature(payload, secret);
        var sig2 = WebhookDispatcher.ComputeSignature(payload, secret);
        var sig3 = WebhookDispatcher.ComputeSignature(payload, "outro-secret");

        Assert.StartsWith("sha256=", sig1);
        Assert.Equal(sig1, sig2); // determinístico
        Assert.NotEqual(sig1, sig3); // secret diferente muda hash
    }

    [Fact]
    public void ComputeSignature_ProduzHexLowercase()
    {
        var sig = WebhookDispatcher.ComputeSignature("teste", "s");
        Assert.Matches("^sha256=[0-9a-f]{64}$", sig);
    }
}
