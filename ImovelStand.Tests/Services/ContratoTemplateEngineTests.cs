using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;

namespace ImovelStand.Tests.Services;

public class ContratoTemplateEngineTests
{
    [Fact]
    public void Substitui_PlaceholderSimples()
    {
        var texto = "Olá {{ cliente.nome }}, CPF {{ cliente.cpf }}.";
        var ctx = new Dictionary<string, object?>
        {
            ["cliente"] = new Cliente { Nome = "João Silva", Cpf = "52998224725" }
        };

        var resultado = ContratoTemplateEngine.SubstituirPlaceholders(texto, ctx);

        Assert.Equal("Olá João Silva, CPF 52998224725.", resultado);
    }

    [Fact]
    public void Substitui_ComNavegacaoEncadeada()
    {
        var texto = "Torre {{ apartamento.torre.nome }}, apto {{ apartamento.numero }}.";
        var torre = new Torre { Nome = "Torre A" };
        var apt = new Apartamento { Numero = "0101", Torre = torre };
        var ctx = new Dictionary<string, object?> { ["apartamento"] = apt };

        var resultado = ContratoTemplateEngine.SubstituirPlaceholders(texto, ctx);

        Assert.Equal("Torre Torre A, apto 0101.", resultado);
    }

    [Fact]
    public void Substitui_ComFormatSpecifierMoeda()
    {
        var texto = "Valor: {{ venda.valorFinal:C2 }}.";
        var ctx = new Dictionary<string, object?>
        {
            ["venda"] = new Venda { ValorFinal = 350_000m }
        };

        var resultado = ContratoTemplateEngine.SubstituirPlaceholders(texto, ctx);

        Assert.Contains("350.000,00", resultado);
    }

    [Fact]
    public void Substitui_ComFormatSpecifierData()
    {
        var texto = "Data: {{ hoje:dd/MM/yyyy }}.";
        var ctx = new Dictionary<string, object?>
        {
            ["hoje"] = new DateTime(2026, 4, 22)
        };

        var resultado = ContratoTemplateEngine.SubstituirPlaceholders(texto, ctx);

        Assert.Equal("Data: 22/04/2026.", resultado);
    }

    [Fact]
    public void Substitui_PropriedadeNull_DeixaVazio()
    {
        var texto = "CPF: {{ cliente.cpf }}, RG: {{ cliente.rg }}.";
        var ctx = new Dictionary<string, object?>
        {
            ["cliente"] = new Cliente { Cpf = "52998224725", Rg = null }
        };

        var resultado = ContratoTemplateEngine.SubstituirPlaceholders(texto, ctx);

        Assert.Equal("CPF: 52998224725, RG: .", resultado);
    }

    [Fact]
    public void Substitui_CaminhoInvalido_DeixaVazio()
    {
        var texto = "{{ nao.existe }}";
        var ctx = new Dictionary<string, object?> { ["outra"] = 1 };

        var resultado = ContratoTemplateEngine.SubstituirPlaceholders(texto, ctx);

        Assert.Equal("", resultado);
    }

    [Fact]
    public void Substitui_DecimalSemFormat_UsaFormatoMoedaPadrao()
    {
        var texto = "{{ valor }}";
        var ctx = new Dictionary<string, object?> { ["valor"] = 1234.56m };

        var resultado = ContratoTemplateEngine.SubstituirPlaceholders(texto, ctx);

        Assert.Contains("1.234,56", resultado);
    }
}
