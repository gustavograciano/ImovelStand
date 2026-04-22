using ImovelStand.Application.Common;

namespace ImovelStand.Tests.Common;

public class DocumentosValidatorTests
{
    [Theory]
    [InlineData("529.982.247-25", true)]
    [InlineData("52998224725", true)]
    [InlineData("111.111.111-11", false)]
    [InlineData("123.456.789-00", false)]
    [InlineData("", false)]
    [InlineData("abc", false)]
    public void CpfValido(string entrada, bool esperado)
    {
        Assert.Equal(esperado, DocumentosValidator.CpfValido(entrada));
    }

    [Theory]
    [InlineData("11.222.333/0001-81", true)]
    [InlineData("11222333000181", true)]
    [InlineData("11.111.111/1111-11", false)]
    [InlineData("", false)]
    public void CnpjValido(string entrada, bool esperado)
    {
        Assert.Equal(esperado, DocumentosValidator.CnpjValido(entrada));
    }

    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("111-222-333-44", "11122233344")]
    [InlineData("abc123", "123")]
    public void NormalizarDigitos(string entrada, string esperado)
    {
        Assert.Equal(esperado, DocumentosValidator.NormalizarDigitos(entrada));
    }
}
