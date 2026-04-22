using ImovelStand.Application.Dtos;
using ImovelStand.Application.Validators;

namespace ImovelStand.Tests.Validators;

public class ClienteValidatorTests
{
    private readonly ClienteCreateRequestValidator _sut = new();

    [Fact]
    public void Validate_RequestValido_ComCpfFormatado_Aprova()
    {
        // CPF 529.982.247-25 é um CPF válido (famoso CPF de exemplo da Receita)
        var r = _sut.Validate(new ClienteCreateRequest
        {
            Nome = "Fulano da Silva",
            Cpf = "529.982.247-25",
            Email = "fulano@example.com",
            Telefone = "11999999999"
        });

        Assert.True(r.IsValid);
    }

    [Fact]
    public void Validate_CpfComDigitoVerificadorInvalido_Rejeita()
    {
        var r = _sut.Validate(new ClienteCreateRequest
        {
            Nome = "Fulano da Silva",
            Cpf = "111.111.111-11", // CPF com todos dígitos iguais
            Email = "fulano@example.com",
            Telefone = "11999999999"
        });

        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == nameof(ClienteCreateRequest.Cpf));
    }

    [Fact]
    public void Validate_EmailInvalido_Rejeita()
    {
        var r = _sut.Validate(new ClienteCreateRequest
        {
            Nome = "Fulano",
            Cpf = "529.982.247-25",
            Email = "nao-e-email",
            Telefone = "11999999999"
        });

        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == nameof(ClienteCreateRequest.Email));
    }

    [Fact]
    public void Validate_NomeMuitoCurto_Rejeita()
    {
        var r = _sut.Validate(new ClienteCreateRequest
        {
            Nome = "Ab",
            Cpf = "529.982.247-25",
            Email = "a@a.com",
            Telefone = "11999999999"
        });

        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == nameof(ClienteCreateRequest.Nome));
    }
}
