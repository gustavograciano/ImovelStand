using ImovelStand.Domain.Enums;

namespace ImovelStand.Application.Dtos;

public class DashboardOverviewResponse
{
    public int EmpreendimentoId { get; set; }
    public string? EmpreendimentoNome { get; set; }
    public int UnidadesTotal { get; set; }
    public int UnidadesDisponiveis { get; set; }
    public int UnidadesReservadas { get; set; }
    public int UnidadesEmProposta { get; set; }
    public int UnidadesVendidas { get; set; }
    public decimal VgvTotal { get; set; }
    public decimal VgvVendido { get; set; }
    public decimal PctVendido { get; set; }
    public decimal PrecoMedioM2 { get; set; }
    public decimal VelocidadeVendaSemanal { get; set; }
    public int VendasUltimos30Dias { get; set; }
    public int VendasUltimos90Dias { get; set; }
}

public class FunilConversaoResponse
{
    public int Leads { get; set; }
    public int Visitas { get; set; }
    public int Propostas { get; set; }
    public int Vendas { get; set; }
    public decimal ConversaoLeadParaVisita { get; set; }
    public decimal ConversaoVisitaParaProposta { get; set; }
    public decimal ConversaoPropostaParaVenda { get; set; }
    public decimal ConversaoGlobal { get; set; }
    public int DiasAnalisados { get; set; }
}

public class RankingCorretorItem
{
    public int CorretorId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int VendasFechadas { get; set; }
    public decimal VgvVendido { get; set; }
    public decimal ComissaoTotal { get; set; }
    public int Visitas { get; set; }
    public decimal TicketMedio { get; set; }
}

public class AlertasDashboard
{
    public List<ReservaExpirando> ReservasExpirando { get; set; } = new();
    public List<PropostaSemResposta> PropostasSemResposta { get; set; } = new();
}

public record ReservaExpirando(int Id, int ClienteId, string ClienteNome, int ApartamentoId, string Numero, DateTime ExpiraEm);
public record PropostaSemResposta(int Id, string Numero, int ClienteId, string ClienteNome, StatusProposta Status, DateTime DataEnvio, int DiasSemResposta);
