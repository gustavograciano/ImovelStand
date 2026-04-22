using FluentValidation;
using ImovelStand.Application.Dtos;

namespace ImovelStand.Application.Validators;

public class ApartamentoCreateRequestValidator : AbstractValidator<ApartamentoCreateRequest>
{
    public ApartamentoCreateRequestValidator()
    {
        RuleFor(x => x.TorreId).GreaterThan(0);
        RuleFor(x => x.TipologiaId).GreaterThan(0);
        RuleFor(x => x.Numero).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Pavimento).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PrecoAtual).GreaterThan(0);
        RuleFor(x => x.Observacoes).MaximumLength(1000);
    }
}

public class ApartamentoUpdateRequestValidator : AbstractValidator<ApartamentoUpdateRequest>
{
    public ApartamentoUpdateRequestValidator()
    {
        RuleFor(x => x.TipologiaId).GreaterThan(0);
        RuleFor(x => x.Numero).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Pavimento).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PrecoAtual).GreaterThan(0);
        RuleFor(x => x.Observacoes).MaximumLength(1000);
    }
}
