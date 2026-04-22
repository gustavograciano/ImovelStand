using ClosedXML.Excel;
using ImovelStand.Application.Services;

namespace ImovelStand.Tests.Services;

public class ExcelImporterTests
{
    private readonly ExcelImporter _sut = new();

    private static Stream BuildPlanilhaPrecos((string torre, string apto, decimal preco)[] rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Dados");
        ws.Cell(1, 1).Value = "Torre";
        ws.Cell(1, 2).Value = "Apto";
        ws.Cell(1, 3).Value = "Preço";
        for (var i = 0; i < rows.Length; i++)
        {
            ws.Cell(i + 2, 1).Value = rows[i].torre;
            ws.Cell(i + 2, 2).Value = rows[i].apto;
            ws.Cell(i + 2, 3).Value = rows[i].preco;
        }
        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    private static Stream BuildPlanilhaClientes((string nome, string cpf, string email)[] rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Clientes");
        ws.Cell(1, 1).Value = "Nome";
        ws.Cell(1, 2).Value = "CPF";
        ws.Cell(1, 3).Value = "Email";
        ws.Cell(1, 4).Value = "Telefone";
        for (var i = 0; i < rows.Length; i++)
        {
            ws.Cell(i + 2, 1).Value = rows[i].nome;
            ws.Cell(i + 2, 2).Value = rows[i].cpf;
            ws.Cell(i + 2, 3).Value = rows[i].email;
            ws.Cell(i + 2, 4).Value = "11999999999";
        }
        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    [Fact]
    public void ParseTabelaPrecos_ComLinhasValidas_RetornaItems()
    {
        var stream = BuildPlanilhaPrecos(new[]
        {
            ("Torre A", "0101", 350_000m),
            ("Torre A", "0102", 520_000m),
            ("Torre B", "1201", 1_200_000m)
        });

        var result = _sut.ParseTabelaPrecos(stream);

        Assert.Equal(3, result.Items.Count);
        Assert.Empty(result.Errors);
        Assert.Equal(350_000m, result.Items[0].NovoPreco);
    }

    [Fact]
    public void ParseTabelaPrecos_ComPrecoInvalido_ReportaErro()
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("X");
        ws.Cell(1, 1).Value = "Torre";
        ws.Cell(1, 2).Value = "Apto";
        ws.Cell(1, 3).Value = "Preço";
        ws.Cell(2, 1).Value = "Torre A";
        ws.Cell(2, 2).Value = "0101";
        ws.Cell(2, 3).Value = "abc"; // preço inválido
        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        var result = _sut.ParseTabelaPrecos(ms);

        Assert.Empty(result.Items);
        Assert.Single(result.Errors);
        Assert.Contains("Preço", result.Errors[0].Mensagem);
    }

    [Fact]
    public void ParseClientes_ComCpfInvalido_ReportaErro()
    {
        var stream = BuildPlanilhaClientes(new[]
        {
            ("Fulano", "529.982.247-25", "a@a.com"), // CPF válido
            ("Ciclano", "111.111.111-11", "b@b.com")   // CPF inválido
        });

        var result = _sut.ParseClientes(stream);

        Assert.Single(result.Items);
        Assert.Single(result.Errors);
        Assert.Contains("CPF", result.Errors[0].Mensagem);
    }

    [Fact]
    public void ParseClientes_NormalizaCpfNoResultado()
    {
        var stream = BuildPlanilhaClientes(new[] { ("Fulano", "529.982.247-25", "a@a.com") });
        var result = _sut.ParseClientes(stream);
        Assert.Equal("52998224725", result.Items[0].Cpf);
    }
}
