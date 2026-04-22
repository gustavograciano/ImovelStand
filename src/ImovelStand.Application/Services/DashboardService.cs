using ImovelStand.Application.Dtos;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;

namespace ImovelStand.Application.Services;

/// <summary>
/// Serviço de cálculo de KPIs para dashboard. Recebe dados já carregados
/// (coleções), o que permite testar sem DbContext e reusar em jobs.
/// </summary>
public class DashboardService
{
    public DashboardOverviewResponse Overview(
        Empreendimento empreendimento,
        IReadOnlyList<Apartamento> apartamentos,
        IReadOnlyList<Tipologia> tipologias,
        IReadOnlyList<Venda> vendasPeriodo,
        DateTime? now = null)
    {
        var dataReferencia = now ?? DateTime.UtcNow;
        var total = apartamentos.Count;
        var vgvTotal = apartamentos.Sum(a => a.PrecoAtual);
        var disponiveis = apartamentos.Count(a => a.Status == StatusApartamento.Disponivel);
        var reservados = apartamentos.Count(a => a.Status == StatusApartamento.Reservado);
        var emProposta = apartamentos.Count(a => a.Status == StatusApartamento.Proposta);
        var vendidos = apartamentos.Count(a => a.Status == StatusApartamento.Vendido);
        var vgvVendido = apartamentos.Where(a => a.Status == StatusApartamento.Vendido).Sum(a => a.PrecoAtual);

        var areaMedia = tipologias.Count == 0 ? 0 : tipologias.Average(t => t.AreaPrivativa);
        var precoMedio = apartamentos.Count == 0 ? 0 : apartamentos.Average(a => a.PrecoAtual);
        var precoMedioM2 = areaMedia == 0 ? 0 : Math.Round(precoMedio / areaMedia, 2);

        var vendasConfirmadas = vendasPeriodo
            .Where(v => v.Status is StatusVenda.EmContrato or StatusVenda.Assinada)
            .ToList();

        var vendasUltimos30 = vendasConfirmadas.Count(v => (dataReferencia - v.DataFechamento).TotalDays <= 30);
        var vendasUltimos90 = vendasConfirmadas.Count(v => (dataReferencia - v.DataFechamento).TotalDays <= 90);
        var velocidadeSemanal = vendasUltimos30 / 4m;

        return new DashboardOverviewResponse
        {
            EmpreendimentoId = empreendimento.Id,
            EmpreendimentoNome = empreendimento.Nome,
            UnidadesTotal = total,
            UnidadesDisponiveis = disponiveis,
            UnidadesReservadas = reservados,
            UnidadesEmProposta = emProposta,
            UnidadesVendidas = vendidos,
            VgvTotal = vgvTotal,
            VgvVendido = vgvVendido,
            PctVendido = total == 0 ? 0 : (decimal)vendidos / total,
            PrecoMedioM2 = precoMedioM2,
            VelocidadeVendaSemanal = Math.Round(velocidadeSemanal, 2),
            VendasUltimos30Dias = vendasUltimos30,
            VendasUltimos90Dias = vendasUltimos90
        };
    }

    public FunilConversaoResponse Funil(
        IReadOnlyList<Cliente> clientes,
        IReadOnlyList<Visita> visitas,
        IReadOnlyList<Proposta> propostas,
        IReadOnlyList<Venda> vendas,
        int diasAnalisados = 90)
    {
        var leads = clientes.Count;
        var comVisita = visitas.Select(v => v.ClienteId).Distinct().Count();
        var comProposta = propostas.Select(p => p.ClienteId).Distinct().Count();
        var comVenda = vendas.Where(v => v.Status is StatusVenda.EmContrato or StatusVenda.Assinada)
            .Select(v => v.ClienteId).Distinct().Count();

        return new FunilConversaoResponse
        {
            DiasAnalisados = diasAnalisados,
            Leads = leads,
            Visitas = comVisita,
            Propostas = comProposta,
            Vendas = comVenda,
            ConversaoLeadParaVisita = leads == 0 ? 0 : (decimal)comVisita / leads,
            ConversaoVisitaParaProposta = comVisita == 0 ? 0 : (decimal)comProposta / comVisita,
            ConversaoPropostaParaVenda = comProposta == 0 ? 0 : (decimal)comVenda / comProposta,
            ConversaoGlobal = leads == 0 ? 0 : (decimal)comVenda / leads
        };
    }

    public List<RankingCorretorItem> Ranking(
        IReadOnlyList<Usuario> corretores,
        IReadOnlyList<Venda> vendas,
        IReadOnlyList<Comissao> comissoes,
        IReadOnlyList<Visita> visitas)
    {
        var ranking = new List<RankingCorretorItem>();
        foreach (var c in corretores)
        {
            var vendasCorretor = vendas.Where(v => v.CorretorId == c.Id
                && (v.Status is StatusVenda.EmContrato or StatusVenda.Assinada)).ToList();
            var vgv = vendasCorretor.Sum(v => v.ValorFinal);
            var comissaoTotal = comissoes.Where(x => x.UsuarioId == c.Id).Sum(x => x.Valor);
            var visitasCorretor = visitas.Count(v => v.CorretorId == c.Id);
            ranking.Add(new RankingCorretorItem
            {
                CorretorId = c.Id,
                Nome = c.Nome,
                VendasFechadas = vendasCorretor.Count,
                VgvVendido = vgv,
                ComissaoTotal = comissaoTotal,
                Visitas = visitasCorretor,
                TicketMedio = vendasCorretor.Count == 0 ? 0 : Math.Round(vgv / vendasCorretor.Count, 2)
            });
        }
        return ranking.OrderByDescending(r => r.VgvVendido).ToList();
    }
}
