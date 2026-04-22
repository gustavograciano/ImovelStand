using FluentValidation;
using ImovelStand.Application.Dtos;

namespace ImovelStand.Application.Validators;

public class CondicaoPagamentoDtoValidator : AbstractValidator<CondicaoPagamentoDto>
{
    public CondicaoPagamentoDtoValidator()
    {
        RuleFor(x => x.ValorTotal).GreaterThan(0);
        RuleFor(x => x.Entrada).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Sinal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ValorChaves).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QtdParcelasMensais).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QtdSemestrais).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QtdPosChaves).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxaJurosAnual).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(c => c.Entrada + c.Sinal + c.ValorChaves
                    + (c.QtdSemestrais * c.ValorSemestral)
                    + (c.QtdPosChaves * c.ValorPosChaves) <= c.ValorTotal + 0.01m)
            .WithMessage("Soma de entrada/sinal/semestrais/chaves/pós-chaves não pode exceder o valor total.");
    }
}

public class PropostaCreateRequestValidator : AbstractValidator<PropostaCreateRequest>
{
    public PropostaCreateRequestValidator()
    {
        RuleFor(x => x.ClienteId).GreaterThan(0);
        RuleFor(x => x.ApartamentoId).GreaterThan(0);
        RuleFor(x => x.CorretorId).GreaterThan(0);
        RuleFor(x => x.ValorOferecido).GreaterThan(0);
        RuleFor(x => x.Condicao).NotNull().SetValidator(new CondicaoPagamentoDtoValidator());
        RuleFor(x => x.DataValidade)
            .Must(d => d is null || d > DateTime.UtcNow)
            .WithMessage("DataValidade deve estar no futuro.");
        RuleFor(x => x.Observacoes).MaximumLength(2000);
    }
}

public class ContrapropostaRequestValidator : AbstractValidator<ContrapropostaRequest>
{
    public ContrapropostaRequestValidator()
    {
        RuleFor(x => x.ValorOferecido).GreaterThan(0);
        RuleFor(x => x.Condicao).NotNull().SetValidator(new CondicaoPagamentoDtoValidator());
    }
}
