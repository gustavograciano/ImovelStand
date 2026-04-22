using ImovelStand.Application.Services;

namespace ImovelStand.Tests.Services;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("Senha@123")]
    [InlineData("MinhaSenh4!")]
    [InlineData("P@ssw0rd")]
    public void Validate_ComSenhaValida_RetornaOk(string senha)
    {
        var result = PasswordPolicy.Validate(senha);
        Assert.True(result.Valid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("", "Senha obrigatória.")]
    [InlineData("curta1!", "ao menos 8")]
    [InlineData("semmaiuscula1!", "letra maiúscula")]
    [InlineData("SEMMINUSCULA1!", "letra minúscula")]
    [InlineData("SemDigito!", "1 dígito")]
    [InlineData("SemEspecial1", "1 caractere especial")]
    public void Validate_ComSenhaInvalida_ContemErroEsperado(string senha, string fragmentoErro)
    {
        var result = PasswordPolicy.Validate(senha);
        Assert.False(result.Valid);
        Assert.Contains(result.Errors, e => e.Contains(fragmentoErro, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Hash_E_Verify_FazemRoundTrip()
    {
        const string senha = "MinhaSenh4!";
        var hash = PasswordPolicy.Hash(senha);

        Assert.NotEqual(senha, hash);
        Assert.True(PasswordPolicy.Verify(senha, hash));
        Assert.False(PasswordPolicy.Verify("outra-senha", hash));
    }

    [Fact]
    public void Hash_UsaBcryptCost12()
    {
        var hash = PasswordPolicy.Hash("MinhaSenh4!");
        // Formato BCrypt: $2a$<cost>$... — cost deve ser 12.
        Assert.StartsWith("$2a$12$", hash);
    }
}
