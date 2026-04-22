using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;

namespace ImovelStand.Tests.Services;

public class PlanEnforcementTests
{
    private static Plano PlanoStarter() => new()
    {
        Id = 1, Nome = "Starter", MaxEmpreendimentos = 1, MaxUnidades = 100, MaxUsuarios = 3
    };

    [Theory]
    [InlineData("empreendimentos", 0, false)]
    [InlineData("empreendimentos", 1, true)] // atinge o limite
    [InlineData("unidades", 99, false)]
    [InlineData("unidades", 100, true)]
    [InlineData("usuarios", 2, false)]
    [InlineData("usuarios", 3, true)]
    [InlineData("inexistente", 9999, false)]
    public void ExcedeLimite(string limite, int valor, bool esperado)
    {
        Assert.Equal(esperado, PlanEnforcement.ExcedeLimite(PlanoStarter(), limite, valor));
    }

    [Fact]
    public void AssinaturaAtiva_Null_Falso()
    {
        Assert.False(PlanEnforcement.AssinaturaAtiva(null));
    }

    [Fact]
    public void AssinaturaAtiva_Trial_Verdadeiro()
    {
        var a = new Assinatura { Status = StatusAssinatura.Trial };
        Assert.True(PlanEnforcement.AssinaturaAtiva(a));
    }

    [Fact]
    public void AssinaturaAtiva_Cancelada_Falso()
    {
        var a = new Assinatura { Status = StatusAssinatura.Cancelada };
        Assert.False(PlanEnforcement.AssinaturaAtiva(a));
    }
}
