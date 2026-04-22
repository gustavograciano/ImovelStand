using System.Text.RegularExpressions;

namespace ImovelStand.Application.Services;

/// <summary>
/// Renderizador simples de placeholders em templates de email/WhatsApp.
/// Sintaxe: <c>{{ chave }}</c> ou <c>{{ objeto.campo }}</c>. Chave ausente
/// vira vazio. Separada do <see cref="ContratoTemplateEngine"/> por não
/// precisar lidar com DOCX.
/// </summary>
public static class TemplateRenderer
{
    private static readonly Regex Placeholder = new(@"\{\{\s*(?<path>[\w\.]+)\s*\}\}", RegexOptions.Compiled);

    public static string Render(string template, IReadOnlyDictionary<string, object?> contexto)
    {
        if (string.IsNullOrEmpty(template)) return template ?? string.Empty;

        return Placeholder.Replace(template, match =>
        {
            var path = match.Groups["path"].Value;
            var partes = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) return string.Empty;

            if (!contexto.TryGetValue(partes[0], out var atual) || atual is null)
                return string.Empty;

            for (int i = 1; i < partes.Length && atual is not null; i++)
            {
                var prop = atual.GetType().GetProperty(partes[i],
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                atual = prop?.GetValue(atual);
            }

            return atual?.ToString() ?? string.Empty;
        });
    }
}
