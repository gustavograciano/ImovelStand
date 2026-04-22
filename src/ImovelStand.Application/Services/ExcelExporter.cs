using ClosedXML.Excel;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;

namespace ImovelStand.Application.Services;

public class ExcelExporter
{
    public byte[] ExportarVendas(IEnumerable<Venda> vendas)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Vendas");

        string[] headers =
        {
            "Numero", "DataFechamento", "DataAprovacao", "Status", "Cliente", "CPF",
            "Apartamento", "Torre", "Tipologia", "Corretor", "ValorFinal",
            "Entrada", "Sinal", "ParcelasMensais", "ValorParcela", "Chaves", "Indice"
        };
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];
        ws.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var v in vendas)
        {
            ws.Cell(row, 1).Value = v.Numero;
            ws.Cell(row, 2).Value = v.DataFechamento;
            ws.Cell(row, 2).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Cell(row, 3).Value = v.DataAprovacao ?? (XLCellValue)Blank.Value;
            ws.Cell(row, 3).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Cell(row, 4).Value = v.Status.ToString();
            ws.Cell(row, 5).Value = v.Cliente?.Nome ?? string.Empty;
            ws.Cell(row, 6).Value = v.Cliente?.Cpf ?? string.Empty;
            ws.Cell(row, 7).Value = v.Apartamento?.Numero ?? string.Empty;
            ws.Cell(row, 8).Value = v.Apartamento?.Torre?.Nome ?? string.Empty;
            ws.Cell(row, 9).Value = v.Apartamento?.Tipologia?.Nome ?? string.Empty;
            ws.Cell(row, 10).Value = v.Corretor?.Nome ?? string.Empty;
            ws.Cell(row, 11).Value = v.ValorFinal;
            ws.Cell(row, 11).Style.NumberFormat.Format = "R$ #,##0.00";
            ws.Cell(row, 12).Value = v.CondicaoFinal.Entrada;
            ws.Cell(row, 13).Value = v.CondicaoFinal.Sinal;
            ws.Cell(row, 14).Value = v.CondicaoFinal.QtdParcelasMensais;
            ws.Cell(row, 15).Value = v.CondicaoFinal.ValorParcelaMensal;
            ws.Cell(row, 16).Value = v.CondicaoFinal.ValorChaves;
            ws.Cell(row, 17).Value = v.CondicaoFinal.Indice.ToString();
            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportarFunilPorOrigem(IEnumerable<Cliente> clientes, IEnumerable<Venda> vendas)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Funil por Origem");

        var porOrigem = clientes
            .GroupBy(c => c.OrigemLead ?? OrigemLead.Outros)
            .OrderBy(g => g.Key)
            .ToList();

        ws.Cell(1, 1).Value = "Origem";
        ws.Cell(1, 2).Value = "Leads";
        ws.Cell(1, 3).Value = "Convertidos em Venda";
        ws.Cell(1, 4).Value = "% Conversão";
        ws.Range("A1:D1").Style.Font.Bold = true;

        var vendasPorCliente = vendas
            .Where(v => v.Status is StatusVenda.EmContrato or StatusVenda.Assinada)
            .Select(v => v.ClienteId)
            .ToHashSet();

        int row = 2;
        foreach (var g in porOrigem)
        {
            var leads = g.Count();
            var convertidos = g.Count(c => vendasPorCliente.Contains(c.Id));
            var pct = leads == 0 ? 0m : (decimal)convertidos / leads;

            ws.Cell(row, 1).Value = g.Key.ToString();
            ws.Cell(row, 2).Value = leads;
            ws.Cell(row, 3).Value = convertidos;
            ws.Cell(row, 4).Value = pct;
            ws.Cell(row, 4).Style.NumberFormat.Format = "0.00%";
            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
