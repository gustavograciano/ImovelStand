using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Xceed.Words.NET;

namespace ImovelStand.Application.Services;

/// <summary>
/// Motor de substituição de placeholders em templates DOCX.
/// Sintaxe: <c>{{ cliente.nome }}</c>, <c>{{ apartamento.pavimento }}</c>,
/// <c>{{ venda.valorFinal:C2 }}</c> (format specifier opcional).
/// Suporta navegação com ponto, números, datas (formato BR) e valores nulos.
/// </summary>
public class ContratoTemplateEngine
{
    private static readonly Regex PlaceholderRegex = new(
        @"\{\{\s*(?<path>[\w\.]+)\s*(?::(?<fmt>[^}]+))?\s*\}\}",
        RegexOptions.Compiled);

    private static readonly CultureInfo Ptbr = new("pt-BR");

    /// <summary>
    /// Aplica substituições sobre um stream de template DOCX e retorna o documento gerado.
    /// </summary>
    public byte[] Render(Stream template, IReadOnlyDictionary<string, object?> contexto)
    {
        template.Position = 0;
        using var output = new MemoryStream();
        template.CopyTo(output);
        output.Position = 0;

        using var doc = DocX.Load(output);

        foreach (var paragrafo in doc.Paragraphs)
        {
            var texto = paragrafo.Text;
            if (!texto.Contains("{{")) continue;

            var novoTexto = SubstituirPlaceholders(texto, contexto);
            if (novoTexto != texto)
            {
                paragrafo.RemoveText(0, texto.Length);
                paragrafo.InsertText(novoTexto);
            }
        }

        using var final = new MemoryStream();
        doc.SaveAs(final);
        return final.ToArray();
    }

    /// <summary>
    /// Versão pública do substituidor — útil para testar o engine sem precisar
    /// gerar DOCX em teste unitário.
    /// </summary>
    public static string SubstituirPlaceholders(string texto, IReadOnlyDictionary<string, object?> contexto)
    {
        return PlaceholderRegex.Replace(texto, match =>
        {
            var path = match.Groups["path"].Value;
            var fmt = match.Groups["fmt"].Value.Trim();
            var valor = ResolverCaminho(path, contexto);
            return Formatar(valor, fmt);
        });
    }

    private static object? ResolverCaminho(string path, IReadOnlyDictionary<string, object?> contexto)
    {
        var partes = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return null;

        if (!contexto.TryGetValue(partes[0], out var atual)) return null;
        if (atual is null) return null;

        for (int i = 1; i < partes.Length && atual is not null; i++)
        {
            atual = ObterPropriedade(atual, partes[i]);
        }
        return atual;
    }

    private static object? ObterPropriedade(object source, string nome)
    {
        // Tenta property, depois field, case-insensitive
        var type = source.GetType();
        var prop = type.GetProperty(nome, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop is not null) return prop.GetValue(source);
        var field = type.GetField(nome, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return field?.GetValue(source);
    }

    private static string Formatar(object? valor, string? fmt)
    {
        if (valor is null) return string.Empty;

        if (!string.IsNullOrWhiteSpace(fmt))
        {
            if (valor is IFormattable f)
                return f.ToString(fmt, Ptbr);
        }

        return valor switch
        {
            DateTime dt => dt.ToString("dd/MM/yyyy", Ptbr),
            decimal d => d.ToString("C2", Ptbr),
            double db => db.ToString("N2", Ptbr),
            _ => valor.ToString() ?? string.Empty
        };
    }
}
