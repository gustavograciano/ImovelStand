using ClosedXML.Excel;
using ImovelStand.Application.Common;
using ImovelStand.Application.Dtos;
using ImovelStand.Domain.Enums;

namespace ImovelStand.Application.Services;

public record ImportResult<T>(
    IReadOnlyList<T> Items,
    IReadOnlyList<ImportError> Errors)
{
    public int Total => Items.Count + Errors.Count;
    public int Validos => Items.Count;
    public int Invalidos => Errors.Count;
}

public record ImportError(int Linha, string Mensagem);

public record TabelaPrecoRow(
    string TorreNome,
    string ApartamentoNumero,
    decimal NovoPreco,
    string? Motivo);

public class ExcelImporter
{
    /// <summary>
    /// Parse de planilha Excel com colunas A=Torre, B=Apto, C=Preco, D=Motivo(opcional).
    /// Primeira linha é cabeçalho. Linhas vazias no meio encerram o parse.
    /// </summary>
    public ImportResult<TabelaPrecoRow> ParseTabelaPrecos(Stream xlsx)
    {
        using var wb = new XLWorkbook(xlsx);
        var ws = wb.Worksheet(1);
        var items = new List<TabelaPrecoRow>();
        var erros = new List<ImportError>();

        var row = 2;
        while (true)
        {
            var torreCell = ws.Cell(row, 1).GetString();
            var aptCell = ws.Cell(row, 2).GetString();
            if (string.IsNullOrWhiteSpace(torreCell) && string.IsNullOrWhiteSpace(aptCell)) break;

            if (string.IsNullOrWhiteSpace(torreCell))
            {
                erros.Add(new ImportError(row, "Torre vazia."));
                row++;
                continue;
            }
            if (string.IsNullOrWhiteSpace(aptCell))
            {
                erros.Add(new ImportError(row, "Número do apartamento vazio."));
                row++;
                continue;
            }

            if (!ws.Cell(row, 3).TryGetValue<decimal>(out var preco) || preco <= 0)
            {
                erros.Add(new ImportError(row, "Preço inválido."));
                row++;
                continue;
            }

            items.Add(new TabelaPrecoRow(torreCell.Trim(), aptCell.Trim(), preco, ws.Cell(row, 4).GetString()));
            row++;
        }

        return new ImportResult<TabelaPrecoRow>(items, erros);
    }

    /// <summary>
    /// Parse de clientes: A=Nome, B=CPF, C=Email, D=Telefone, E=OrigemLead(opcional).
    /// Valida CPF via DocumentosValidator; duplicatas detectadas pelo controller
    /// (o importer só parseia + valida formato).
    /// </summary>
    public ImportResult<ClienteCreateRequest> ParseClientes(Stream xlsx)
    {
        using var wb = new XLWorkbook(xlsx);
        var ws = wb.Worksheet(1);
        var items = new List<ClienteCreateRequest>();
        var erros = new List<ImportError>();

        var row = 2;
        while (true)
        {
            var nome = ws.Cell(row, 1).GetString();
            var cpf = ws.Cell(row, 2).GetString();
            if (string.IsNullOrWhiteSpace(nome) && string.IsNullOrWhiteSpace(cpf)) break;

            var email = ws.Cell(row, 3).GetString();
            var telefone = ws.Cell(row, 4).GetString();

            if (string.IsNullOrWhiteSpace(nome))
            {
                erros.Add(new ImportError(row, "Nome obrigatório."));
                row++;
                continue;
            }
            if (!DocumentosValidator.CpfValido(cpf))
            {
                erros.Add(new ImportError(row, $"CPF inválido: {cpf}."));
                row++;
                continue;
            }
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                erros.Add(new ImportError(row, "Email inválido."));
                row++;
                continue;
            }

            var origemStr = ws.Cell(row, 5).GetString();
            OrigemLead? origem = null;
            if (!string.IsNullOrWhiteSpace(origemStr) && Enum.TryParse<OrigemLead>(origemStr, true, out var o))
                origem = o;

            items.Add(new ClienteCreateRequest
            {
                Nome = nome.Trim(),
                Cpf = DocumentosValidator.NormalizarDigitos(cpf),
                Email = email.Trim(),
                Telefone = string.IsNullOrWhiteSpace(telefone) ? "" : telefone.Trim(),
                OrigemLead = origem
            });
            row++;
        }

        return new ImportResult<ClienteCreateRequest>(items, erros);
    }
}
