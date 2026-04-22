using FluentValidation;
using ImovelStand.Application.Dtos;

namespace ImovelStand.Application.Validators;

public class VendaCreateRequestValidator : AbstractValidator<VendaCreateRequest>
{
    public VendaCreateRequestValidator()
    {
        RuleFor(x => x.ClienteId).GreaterThan(0);
        RuleFor(x => x.ApartamentoId).GreaterThan(0);
        RuleFor(x => x.CorretorId).GreaterThan(0);
        RuleFor(x => x.ValorFinal).GreaterThan(0);
        RuleFor(x => x.CondicaoFinal).NotNull().SetValidator(new CondicaoPagamentoDtoValidator());
        RuleFor(x => x.Observacoes).MaximumLength(1000);
        RuleFor(x => x.CorretorCaptacaoId)
            .Must((req, capt) => capt is null || capt != req.CorretorId)
            .WithMessage("CorretorCaptacao e Corretor da venda não podem ser a mesma pessoa (comissão só aparece se forem distintos).");
    }
}

public class VisitaCreateRequestValidator : AbstractValidator<VisitaCreateRequest>
{
    public VisitaCreateRequestValidator()
    {
        RuleFor(x => x.ClienteId).GreaterThan(0);
        RuleFor(x => x.CorretorId).GreaterThan(0);
        RuleFor(x => x.EmpreendimentoId).GreaterThan(0);
        RuleFor(x => x.DataHora).NotEmpty();
        RuleFor(x => x.DuracaoMinutos).GreaterThan(0).When(x => x.DuracaoMinutos.HasValue);
        RuleFor(x => x.Observacoes).MaximumLength(1000);
    }
}

public class InteracaoCreateRequestValidator : AbstractValidator<InteracaoCreateRequest>
{
    public InteracaoCreateRequestValidator()
    {
        RuleFor(x => x.Conteudo).NotEmpty().MinimumLength(1).MaximumLength(2000);
    }
}
