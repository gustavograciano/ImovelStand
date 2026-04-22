using System.Text.RegularExpressions;

namespace ImovelStand.Application.Services;

public static class PasswordPolicy
{
    /// <summary>BCrypt workfactor — custo adequado em CPU moderna 2025.</summary>
    public const int BcryptWorkFactor = 12;

    public const int MinimumLength = 8;

    private static readonly Regex Uppercase = new("[A-Z]", RegexOptions.Compiled);
    private static readonly Regex Lowercase = new("[a-z]", RegexOptions.Compiled);
    private static readonly Regex Digit = new("[0-9]", RegexOptions.Compiled);
    private static readonly Regex Special = new("[^A-Za-z0-9]", RegexOptions.Compiled);

    public static PasswordValidationResult Validate(string? password)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            erros.Add("Senha obrigatória.");
            return new PasswordValidationResult(false, erros);
        }

        if (password.Length < MinimumLength) erros.Add($"Senha deve ter ao menos {MinimumLength} caracteres.");
        if (!Uppercase.IsMatch(password)) erros.Add("Senha deve conter ao menos 1 letra maiúscula.");
        if (!Lowercase.IsMatch(password)) erros.Add("Senha deve conter ao menos 1 letra minúscula.");
        if (!Digit.IsMatch(password)) erros.Add("Senha deve conter ao menos 1 dígito.");
        if (!Special.IsMatch(password)) erros.Add("Senha deve conter ao menos 1 caractere especial.");

        return new PasswordValidationResult(erros.Count == 0, erros);
    }

    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BcryptWorkFactor);
    }

    public static bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

public record PasswordValidationResult(bool Valid, IReadOnlyList<string> Errors);
