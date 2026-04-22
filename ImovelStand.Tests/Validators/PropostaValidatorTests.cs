using ImovelStand.Application.Dtos;
using ImovelStand.Application.Validators;
using ImovelStand.Domain.Enums;

namespace ImovelStand.Tests.Validators;

public class PropostaValidatorTests
{
    private static CondicaoPagamentoDto CondicaoValida() => new()
    {
        ValorTotal = 500_000m,
        Entrada = 100_000m,
        Sinal = 20_000m,
        QtdParcelasMensais = 60,
        ValorParcelaMensal = 6_000m,
        ValorChaves = 20_000m,
        Indice = IndiceReajuste.Incc
    };

    [Fact]
    public void PropostaCreate_ValorOferecidoZero_Rejeita()
    {
        var sut = new PropostaCreateRequestValidator();
        var r = sut.Validate(new PropostaCreateRequest
        {
            ClienteId = 1, ApartamentoId = 1, CorretorId = 1,
            ValorOferecido = 0,
            Condicao = CondicaoValida()
        });
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == nameof(PropostaCreateRequest.ValorOferecido));
    }

    [Fact]
    public void PropostaCreate_DataValidadeNoPassado_Rejeita()
    {
        var sut = new PropostaCreateRequestValidator();
        var r = sut.Validate(new PropostaCreateRequest
        {
            ClienteId = 1, ApartamentoId = 1, CorretorId = 1,
            ValorOferecido = 500_000m,
            Condicao = CondicaoValida(),
            DataValidade = DateTime.UtcNow.AddDays(-1)
        });
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == nameof(PropostaCreateRequest.DataValidade));
    }

    [Fact]
    public void Condicao_FixadosExcedemTotal_Rejeita()
    {
        var sut = new CondicaoPagamentoDtoValidator();
        var r = sut.Validate(new CondicaoPagamentoDto
        {
            ValorTotal = 100_000m,
            Entrada = 150_000m, // excede o total
            QtdParcelasMensais = 10,
            ValorParcelaMensal = 1_000m,
            Indice = IndiceReajuste.SemReajuste
        });
        Assert.False(r.IsValid);
    }

    [Fact]
    public void VendaCreate_CorretorEqCaptacao_Rejeita()
    {
        var sut = new VendaCreateRequestValidator();
        var r = sut.Validate(new VendaCreateRequest
        {
            ClienteId = 1, ApartamentoId = 1, CorretorId = 5, CorretorCaptacaoId = 5,
            ValorFinal = 400_000m,
            CondicaoFinal = CondicaoValida()
        });
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == nameof(VendaCreateRequest.CorretorCaptacaoId));
    }

    [Fact]
    public void UsuarioCreate_SenhaFraca_Rejeita()
    {
        var sut = new UsuarioCreateRequestValidator();
        var r = sut.Validate(new UsuarioCreateRequest
        {
            Nome = "Fulano", Email = "a@a.com", Senha = "fraca", Role = "Corretor"
        });
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == nameof(UsuarioCreateRequest.Senha));
    }

    [Fact]
    public void UsuarioCreate_PercentualForaRange_Rejeita()
    {
        var sut = new UsuarioCreateRequestValidator();
        var r = sut.Validate(new UsuarioCreateRequest
        {
            Nome = "Fulano", Email = "a@a.com", Senha = "MinhaSenh4!", Role = "Corretor",
            PercentualComissao = 1.5m // 150%
        });
        Assert.False(r.IsValid);
    }

    [Fact]
    public void InteracaoCreate_ConteudoVazio_Rejeita()
    {
        var sut = new InteracaoCreateRequestValidator();
        var r = sut.Validate(new InteracaoCreateRequest { Tipo = TipoInteracao.Whatsapp, Conteudo = "" });
        Assert.False(r.IsValid);
    }
}
