using ImovelStand.Domain.Enums;

namespace ImovelStand.Application.Dtos;

public record HeatmapCelula(
    int Pavimento,
    string Numero,
    string Torre,
    StatusApartamento Status,
    decimal Preco,
    int ApartamentoId
);

public class HeatmapResponse
{
    public int EmpreendimentoId { get; set; }
    public List<string> Torres { get; set; } = new();
    public List<HeatmapCelula> Celulas { get; set; } = new();
}
