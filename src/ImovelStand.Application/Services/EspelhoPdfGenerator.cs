using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ImovelStand.Application.Services;

public enum TipoEspelho
{
    Comercial = 0,
    PorTorre = 1,
    Executivo = 2
}

public record EspelhoMetadata(string TenantNome, string GeradoPor, DateTime GeradoEm);

public class EspelhoPdfGenerator
{
    static EspelhoPdfGenerator()
    {
        // QuestPDF Community: grátis para empresas com receita < US$1M/ano
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Gerar(
        TipoEspelho tipo,
        Empreendimento empreendimento,
        IReadOnlyList<Torre> torres,
        IReadOnlyList<Tipologia> tipologias,
        IReadOnlyList<Apartamento> apartamentos,
        EspelhoMetadata metadata)
    {
        return tipo switch
        {
            TipoEspelho.Comercial => GerarComercial(empreendimento, tipologias, apartamentos, metadata),
            TipoEspelho.PorTorre => GerarPorTorre(empreendimento, torres, tipologias, apartamentos, metadata),
            TipoEspelho.Executivo => GerarExecutivo(empreendimento, torres, tipologias, apartamentos, metadata),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo))
        };
    }

    private byte[] GerarComercial(
        Empreendimento emp, IReadOnlyList<Tipologia> tipologias, IReadOnlyList<Apartamento> apts, EspelhoMetadata meta)
    {
        var tipologiaById = tipologias.ToDictionary(t => t.Id);

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(c =>
                {
                    c.Item().Text($"ESPELHO COMERCIAL — {emp.Nome}").Bold().FontSize(16);
                    c.Item().Text($"Tenant: {meta.TenantNome} · Gerado em {meta.GeradoEm:dd/MM/yyyy HH:mm} por {meta.GeradoPor}")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });

                page.Content().Element(content =>
                {
                    content.PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(cd =>
                        {
                            cd.ConstantColumn(50);  // apto
                            cd.ConstantColumn(50);  // pav
                            cd.RelativeColumn(2);   // tipologia
                            cd.ConstantColumn(50);  // quartos
                            cd.ConstantColumn(60);  // area
                            cd.RelativeColumn();    // preco
                            cd.ConstantColumn(60);  // status
                        });

                        table.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).Text("Apto");
                            h.Cell().Element(HeaderCell).Text("Pav");
                            h.Cell().Element(HeaderCell).Text("Tipologia");
                            h.Cell().Element(HeaderCell).Text("Quartos");
                            h.Cell().Element(HeaderCell).Text("Área (m²)");
                            h.Cell().Element(HeaderCell).Text("Preço");
                            h.Cell().Element(HeaderCell).Text("Status");
                        });

                        foreach (var apt in apts.OrderBy(a => a.Pavimento).ThenBy(a => a.Numero))
                        {
                            var tipologia = tipologiaById.GetValueOrDefault(apt.TipologiaId);
                            table.Cell().Element(BodyCell).Text(apt.Numero);
                            table.Cell().Element(BodyCell).Text(apt.Pavimento.ToString());
                            table.Cell().Element(BodyCell).Text(tipologia?.Nome ?? "-");
                            table.Cell().Element(BodyCell).Text((tipologia?.Quartos ?? 0).ToString());
                            table.Cell().Element(BodyCell).Text($"{tipologia?.AreaPrivativa:0.##}");
                            table.Cell().Element(BodyCell).AlignRight().Text(apt.PrecoAtual.ToString("C2"));
                            table.Cell().Element(BodyCell).Element(c => AddStatusBadge(c, apt.Status));
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("CONFIDENCIAL · ").FontColor(Colors.Red.Medium).Bold();
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private byte[] GerarPorTorre(
        Empreendimento emp, IReadOnlyList<Torre> torres, IReadOnlyList<Tipologia> tipologias,
        IReadOnlyList<Apartamento> apts, EspelhoMetadata meta)
    {
        return Document.Create(doc =>
        {
            foreach (var torre in torres)
            {
                var aptsDaTorre = apts.Where(a => a.TorreId == torre.Id).ToList();
                var porPavimento = aptsDaTorre.GroupBy(a => a.Pavimento)
                    .OrderByDescending(g => g.Key)
                    .ToList();

                doc.Page(page =>
                {
                    page.Size(PageSizes.A3.Landscape());
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(c =>
                    {
                        c.Item().Text($"ESPELHO — {emp.Nome} · {torre.Nome}").Bold().FontSize(18);
                        c.Item().Text($"{torre.Pavimentos} pavimentos · {torre.ApartamentosPorPavimento} apartamentos por pavimento");
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        foreach (var pav in porPavimento)
                        {
                            col.Item().PaddingVertical(2).Row(row =>
                            {
                                row.ConstantItem(60).Element(c => c.Background(Colors.Grey.Lighten3).Padding(5))
                                    .AlignCenter().Text($"Pav {pav.Key:00}").Bold();

                                foreach (var apt in pav.OrderBy(a => a.Numero))
                                {
                                    row.RelativeItem().Padding(3).Element(c =>
                                    {
                                        c.Background(StatusColor(apt.Status))
                                         .Padding(8)
                                         .Column(inner =>
                                         {
                                             inner.Item().Text(apt.Numero).FontSize(11).Bold().FontColor(Colors.White);
                                             inner.Item().Text(apt.PrecoAtual.ToString("C0")).FontSize(8).FontColor(Colors.White);
                                         });
                                    });
                                }
                            });
                        }

                        col.Item().PaddingTop(15).Row(r =>
                        {
                            r.ConstantItem(150).Text("Legenda:").Bold();
                            LegendaItem(r, StatusApartamento.Disponivel, "Disponível");
                            LegendaItem(r, StatusApartamento.Reservado, "Reservado");
                            LegendaItem(r, StatusApartamento.Proposta, "Em proposta");
                            LegendaItem(r, StatusApartamento.Vendido, "Vendido");
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span($"CONFIDENCIAL · {meta.GeradoEm:dd/MM/yyyy HH:mm} · ").FontColor(Colors.Red.Medium).Bold();
                        x.Span(meta.GeradoPor);
                    });
                });
            }
        }).GeneratePdf();
    }

    private byte[] GerarExecutivo(
        Empreendimento emp, IReadOnlyList<Torre> torres, IReadOnlyList<Tipologia> tipologias,
        IReadOnlyList<Apartamento> apts, EspelhoMetadata meta)
    {
        var totalUnidades = apts.Count;
        var vendidas = apts.Count(a => a.Status == StatusApartamento.Vendido);
        var reservadas = apts.Count(a => a.Status == StatusApartamento.Reservado);
        var emProposta = apts.Count(a => a.Status == StatusApartamento.Proposta);
        var disponiveis = apts.Count(a => a.Status == StatusApartamento.Disponivel);
        var vgvTotal = apts.Sum(a => a.PrecoAtual);
        var vgvVendido = apts.Where(a => a.Status == StatusApartamento.Vendido).Sum(a => a.PrecoAtual);
        var pctVendido = totalUnidades == 0 ? 0 : (decimal)vendidas / totalUnidades;
        var areaMedia = tipologias.Count == 0 ? 0 : tipologias.Average(t => t.AreaPrivativa);
        var precoMedioM2 = areaMedia == 0 ? 0 : apts.Where(a => a.Status != StatusApartamento.Vendido).Select(a => a.PrecoAtual).DefaultIfEmpty(0).Average() / areaMedia;

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(c =>
                {
                    c.Item().Text($"ESPELHO EXECUTIVO — {emp.Nome}").Bold().FontSize(18);
                    c.Item().Text($"{meta.GeradoEm:dd/MM/yyyy HH:mm} · {meta.TenantNome}").FontColor(Colors.Grey.Medium).FontSize(9);
                });

                page.Content().PaddingTop(15).Column(col =>
                {
                    col.Item().PaddingBottom(15).Row(row =>
                    {
                        KpiCard(row, "VGV Total", vgvTotal.ToString("C0"), Colors.Blue.Medium);
                        KpiCard(row, "VGV Vendido", vgvVendido.ToString("C0"), Colors.Green.Medium);
                        KpiCard(row, "% Vendido", pctVendido.ToString("P1"), Colors.Orange.Medium);
                        KpiCard(row, "Preço médio/m²", precoMedioM2.ToString("C0"), Colors.Purple.Medium);
                    });

                    col.Item().Text("Distribuição por status").Bold().FontSize(14);
                    col.Item().PaddingVertical(5).Table(table =>
                    {
                        table.ColumnsDefinition(cd => { cd.RelativeColumn(); cd.ConstantColumn(80); cd.ConstantColumn(80); });
                        table.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).Text("Status");
                            h.Cell().Element(HeaderCell).Text("Qtd");
                            h.Cell().Element(HeaderCell).Text("%");
                        });
                        LinhaStatus(table, "Disponíveis", disponiveis, totalUnidades);
                        LinhaStatus(table, "Reservados", reservadas, totalUnidades);
                        LinhaStatus(table, "Em proposta", emProposta, totalUnidades);
                        LinhaStatus(table, "Vendidos", vendidas, totalUnidades);
                    });

                    col.Item().PaddingTop(15).Text("Distribuição por torre").Bold().FontSize(14);
                    foreach (var torre in torres)
                    {
                        var aptsTorre = apts.Where(a => a.TorreId == torre.Id).ToList();
                        var vendidasTorre = aptsTorre.Count(a => a.Status == StatusApartamento.Vendido);
                        col.Item().PaddingTop(5).Text($"{torre.Nome}: {aptsTorre.Count} unidades · {vendidasTorre} vendidas");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("CONFIDENCIAL · ").FontColor(Colors.Red.Medium).Bold();
                    x.Span($"Gerado por {meta.GeradoPor}");
                });
            });
        }).GeneratePdf();
    }

    // --- helpers ---

    private static void LinhaStatus(TableDescriptor table, string label, int qtd, int total)
    {
        var pct = total == 0 ? 0 : (decimal)qtd / total;
        table.Cell().Element(BodyCell).Text(label);
        table.Cell().Element(BodyCell).AlignRight().Text(qtd.ToString());
        table.Cell().Element(BodyCell).AlignRight().Text(pct.ToString("P1"));
    }

    private static IContainer BodyCell(IContainer c) =>
        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4).PaddingHorizontal(4);

    private static IContainer HeaderCell(IContainer c) =>
        c.Background(Colors.Grey.Lighten3).BorderBottom(1).PaddingVertical(5).PaddingHorizontal(4).DefaultTextStyle(x => x.Bold());

    private static string StatusColor(StatusApartamento status) => status switch
    {
        StatusApartamento.Disponivel => Colors.Green.Medium,
        StatusApartamento.Reservado => Colors.Amber.Medium,
        StatusApartamento.Proposta => Colors.Orange.Medium,
        StatusApartamento.Vendido => Colors.Red.Medium,
        StatusApartamento.Bloqueado => Colors.Grey.Medium,
        _ => Colors.Grey.Lighten2
    };

    private static void AddStatusBadge(IContainer c, StatusApartamento status)
    {
        c.AlignRight().Background(StatusColor(status)).PaddingHorizontal(6).PaddingVertical(2)
            .Text(status.ToString()).FontColor(Colors.White).FontSize(8);
    }

    private static void LegendaItem(RowDescriptor row, StatusApartamento status, string label)
    {
        row.ConstantItem(120).Row(r =>
        {
            r.ConstantItem(18).Height(14).Background(StatusColor(status));
            r.RelativeItem().PaddingLeft(5).AlignMiddle().Text(label).FontSize(9);
        });
    }

    private static void KpiCard(RowDescriptor row, string label, string valor, string cor)
    {
        row.RelativeItem().Padding(5).Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
        {
            col.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
            col.Item().PaddingTop(4).Text(valor).FontSize(14).Bold().FontColor(cor);
        });
    }
}
