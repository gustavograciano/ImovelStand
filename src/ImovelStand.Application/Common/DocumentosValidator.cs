using System.Text.RegularExpressions;

namespace ImovelStand.Application.Common;

/// <summary>
/// Validações de documentos brasileiros (CPF e CNPJ). Não faz consulta à Receita,
/// apenas verifica formato + dígitos verificadores.
/// </summary>
public static class DocumentosValidator
{
    private static readonly Regex OnlyDigits = new("[^0-9]", RegexOptions.Compiled);

    public static string NormalizarDigitos(string? doc) =>
        string.IsNullOrWhiteSpace(doc) ? string.Empty : OnlyDigits.Replace(doc, "");

    public static bool CpfValido(string? cpf)
    {
        var digitos = NormalizarDigitos(cpf);
        if (digitos.Length != 11) return false;
        if (digitos.Distinct().Count() == 1) return false; // 11111111111 etc

        var numeros = digitos.Select(c => c - '0').ToArray();

        var dv1 = CalcularDv(numeros, 9, 10);
        if (numeros[9] != dv1) return false;

        var dv2 = CalcularDv(numeros, 10, 11);
        return numeros[10] == dv2;
    }

    public static bool CnpjValido(string? cnpj)
    {
        var digitos = NormalizarDigitos(cnpj);
        if (digitos.Length != 14) return false;
        if (digitos.Distinct().Count() == 1) return false;

        var numeros = digitos.Select(c => c - '0').ToArray();

        int[] pesos1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] pesos2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var dv1 = CalcularDvCnpj(numeros, 12, pesos1);
        if (numeros[12] != dv1) return false;

        var dv2 = CalcularDvCnpj(numeros, 13, pesos2);
        return numeros[13] == dv2;
    }

    private static int CalcularDv(int[] numeros, int len, int pesoInicial)
    {
        int soma = 0;
        for (int i = 0; i < len; i++) soma += numeros[i] * (pesoInicial - i);
        int resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static int CalcularDvCnpj(int[] numeros, int len, int[] pesos)
    {
        int soma = 0;
        for (int i = 0; i < len; i++) soma += numeros[i] * pesos[i];
        int resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
