using FluentValidation;
using ImovelStand.Application.Common;
using ImovelStand.Application.Dtos;

namespace ImovelStand.Application.Validators;

public class ClienteCreateRequestValidator : AbstractValidator<ClienteCreateRequest>
{
    public ClienteCreateRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MinimumLength(3).MaximumLength(200);
        RuleFor(x => x.Cpf)
            .NotEmpty()
            .Must(DocumentosValidator.CpfValido)
            .WithMessage("CPF inválido.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(x => x.Telefone).NotEmpty().MaximumLength(20);
    }
}

public class ClienteUpdateRequestValidator : AbstractValidator<ClienteUpdateRequest>
{
    public ClienteUpdateRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MinimumLength(3).MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(x => x.Telefone).NotEmpty().MaximumLength(20);
    }
}
