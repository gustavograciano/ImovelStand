using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;

namespace ImovelStand.Tests.Services;

public class EspelhoPdfGeneratorTests
{
    private readonly EspelhoPdfGenerator _sut = new();

    private static (Empreendimento emp, List<Torre> torres, List<Tipologia> tipologias, List<Apartamento> apts) BuildCenario()
    {
        var emp = new Empreendimento { Id = 1, Nome = "Residencial Exemplo", Slug = "residencial-exemplo" };
        var torres = new List<Torre>
        {
            new() { Id = 1, EmpreendimentoId = 1, Nome = "Torre A", Pavimentos = 4, ApartamentosPorPavimento = 2 }
        };
        var tipologias = new List<Tipologia>
        {
            new() { Id = 1, EmpreendimentoId = 1, Nome = "2Q", AreaPrivativa = 55, AreaTotal = 70, Quartos = 2, Banheiros = 1, PrecoBase = 350_000 },
            new() { Id = 2, EmpreendimentoId = 1, Nome = "3Q", AreaPrivativa = 75, AreaTotal = 95, Quartos = 3, Banheiros = 2, PrecoBase = 520_000 }
        };
        var apts = new List<Apartamento>();
        for (var pav = 1; pav <= 4; pav++)
        {
            for (var u = 1; u <= 2; u++)
            {
                var tipId = u == 1 ? 1 : 2;
                apts.Add(new Apartamento
                {
                    Id = apts.Count + 1,
                    TorreId = 1,
                    TipologiaId = tipId,
                    Numero = $"{pav:00}{u:00}",
                    Pavimento = pav,
                    PrecoAtual = tipId == 1 ? 350_000m : 520_000m,
                    Status = pav == 1 ? StatusApartamento.Vendido : StatusApartamento.Disponivel
                });
            }
        }
        return (emp, torres, tipologias, apts);
    }

    [Theory]
    [InlineData(TipoEspelho.Comercial)]
    [InlineData(TipoEspelho.PorTorre)]
    [InlineData(TipoEspelho.Executivo)]
    public void Gerar_ProduzPdfNaoVazio(TipoEspelho tipo)
    {
        var (emp, torres, tipologias, apts) = BuildCenario();
        var metadata = new EspelhoMetadata("Tenant Demo", "user@test.com", new DateTime(2026, 4, 22, 10, 0, 0, DateTimeKind.Utc));

        var pdf = _sut.Gerar(tipo, emp, torres, tipologias, apts, metadata);

        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 1000, "PDF de verdade tem mais de 1KB.");
        // Magic bytes: %PDF
        Assert.Equal(0x25, pdf[0]);
        Assert.Equal(0x50, pdf[1]);
        Assert.Equal(0x44, pdf[2]);
        Assert.Equal(0x46, pdf[3]);
    }

    [Fact]
    public void Gerar_SemApartamentos_AindaGeraPdfValido()
    {
        var (emp, torres, tipologias, _) = BuildCenario();
        var metadata = new EspelhoMetadata("Tenant Demo", "user", DateTime.UtcNow);

        var pdf = _sut.Gerar(TipoEspelho.Executivo, emp, torres, tipologias, new List<Apartamento>(), metadata);

        Assert.True(pdf.Length > 500);
    }

    [Fact]
    public void Gerar_TipoInvalido_Lanca()
    {
        var (emp, torres, tipologias, apts) = BuildCenario();
        var metadata = new EspelhoMetadata("x", "x", DateTime.UtcNow);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _sut.Gerar((TipoEspelho)99, emp, torres, tipologias, apts, metadata));
    }
}
