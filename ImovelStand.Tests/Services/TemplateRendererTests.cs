using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;

namespace ImovelStand.Tests.Services;

public class TemplateRendererTests
{
    [Fact]
    public void Render_SubstituiChaveSimples()
    {
        var result = TemplateRenderer.Render(
            "Olá {{ nome }}",
            new Dictionary<string, object?> { ["nome"] = "Fulano" });
        Assert.Equal("Olá Fulano", result);
    }

    [Fact]
    public void Render_ComNavegacao()
    {
        var result = TemplateRenderer.Render(
            "{{ cliente.nome }} - {{ cliente.cpf }}",
            new Dictionary<string, object?>
            {
                ["cliente"] = new Cliente { Nome = "Fulano", Cpf = "12345678900" }
            });
        Assert.Equal("Fulano - 12345678900", result);
    }

    [Fact]
    public void Render_ChaveAusenteViraVazio()
    {
        var result = TemplateRenderer.Render(
            "{{ existe }}-{{ nao_existe }}",
            new Dictionary<string, object?> { ["existe"] = "X" });
        Assert.Equal("X-", result);
    }

    [Fact]
    public void Render_TemplateVazio_RetornaVazio()
    {
        Assert.Equal("", TemplateRenderer.Render("", new Dictionary<string, object?>()));
    }

    [Fact]
    public void Render_SemPlaceholders_RetornaOriginal()
    {
        var t = "Sem placeholder algum";
        Assert.Equal(t, TemplateRenderer.Render(t, new Dictionary<string, object?>()));
    }
}
