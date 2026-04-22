using FluentValidation;
using ImovelStand.Application.Dtos;
using ImovelStand.Application.Services;

namespace ImovelStand.Application.Validators;

public class UsuarioCreateRequestValidator : AbstractValidator<UsuarioCreateRequest>
{
    private static readonly string[] RolesValidos = { "Admin", "Gerente", "Corretor", "Financeiro" };

    public UsuarioCreateRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(x => x.Senha)
            .Must(s => PasswordPolicy.Validate(s).Valid)
            .WithMessage("Senha não atende à política (8 chars, maiúscula, minúscula, dígito, especial).");
        RuleFor(x => x.Role).Must(r => RolesValidos.Contains(r)).WithMessage("Role inválido.");
        RuleFor(x => x.Creci).MaximumLength(20);
        RuleFor(x => x.PercentualComissao)
            .InclusiveBetween(0m, 1m)
            .When(x => x.PercentualComissao.HasValue)
            .WithMessage("Percentual de comissão deve ser entre 0 e 1 (ex: 0.03 = 3%).");
    }
}

public class UsuarioUpdateRequestValidator : AbstractValidator<UsuarioUpdateRequest>
{
    private static readonly string[] RolesValidos = { "Admin", "Gerente", "Corretor", "Financeiro" };

    public UsuarioUpdateRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Role).Must(r => RolesValidos.Contains(r));
        RuleFor(x => x.PercentualComissao).InclusiveBetween(0m, 1m).When(x => x.PercentualComissao.HasValue);
    }
}

public class TrocarSenhaRequestValidator : AbstractValidator<TrocarSenhaRequest>
{
    public TrocarSenhaRequestValidator()
    {
        RuleFor(x => x.SenhaAtual).NotEmpty();
        RuleFor(x => x.NovaSenha)
            .Must(s => PasswordPolicy.Validate(s).Valid)
            .WithMessage("Nova senha não atende à política.");
    }
}
