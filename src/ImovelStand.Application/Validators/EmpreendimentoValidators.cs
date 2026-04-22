using FluentValidation;
using ImovelStand.Application.Dtos;

namespace ImovelStand.Application.Validators;

public class EnderecoDtoValidator : AbstractValidator<EnderecoDto>
{
    public EnderecoDtoValidator()
    {
        RuleFor(x => x.Logradouro).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Numero).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Bairro).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Cidade).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Uf).NotEmpty().Length(2).Matches("^[A-Z]{2}$").WithMessage("UF deve ter 2 letras maiúsculas.");
        RuleFor(x => x.Cep).NotEmpty().Matches(@"^\d{5}-?\d{3}$").WithMessage("CEP inválido.");
    }
}

public class EmpreendimentoCreateRequestValidator : AbstractValidator<EmpreendimentoCreateRequest>
{
    public EmpreendimentoCreateRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MinimumLength(3).MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(220)
            .Matches("^[a-z0-9-]+$").WithMessage("Slug deve conter apenas letras minúsculas, números e hífens.");
        RuleFor(x => x.Descricao).MaximumLength(2000);
        RuleFor(x => x.Construtora).MaximumLength(200);
        RuleFor(x => x.Endereco).NotNull().SetValidator(new EnderecoDtoValidator());
        RuleFor(x => x.VgvEstimado).GreaterThan(0).When(x => x.VgvEstimado.HasValue);
        RuleFor(x => x)
            .Must(e => !e.DataEntregaPrevista.HasValue || !e.DataLancamento.HasValue
                    || e.DataEntregaPrevista > e.DataLancamento)
            .WithMessage("DataEntregaPrevista deve ser posterior a DataLancamento.");
    }
}

public class EmpreendimentoUpdateRequestValidator : AbstractValidator<EmpreendimentoUpdateRequest>
{
    public EmpreendimentoUpdateRequestValidator()
    {
        Include(new EmpreendimentoCreateRequestValidator());
    }
}
