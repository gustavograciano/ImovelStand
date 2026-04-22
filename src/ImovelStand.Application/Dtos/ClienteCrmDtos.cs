using ImovelStand.Domain.Enums;

namespace ImovelStand.Application.Dtos;

public class InteracaoCreateRequest
{
    public TipoInteracao Tipo { get; set; }
    public string Conteudo { get; set; } = string.Empty;
}

public class InteracaoResponse
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int? UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
    public TipoInteracao Tipo { get; set; }
    public string Conteudo { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
}

public class VisitaCreateRequest
{
    public int ClienteId { get; set; }
    public int CorretorId { get; set; }
    public int EmpreendimentoId { get; set; }
    public DateTime DataHora { get; set; }
    public int? DuracaoMinutos { get; set; }
    public string? Observacoes { get; set; }
    public bool GerouProposta { get; set; }
}

public class VisitaResponse
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string? ClienteNome { get; set; }
    public int CorretorId { get; set; }
    public string? CorretorNome { get; set; }
    public int EmpreendimentoId { get; set; }
    public string? EmpreendimentoNome { get; set; }
    public DateTime DataHora { get; set; }
    public int? DuracaoMinutos { get; set; }
    public string? Observacoes { get; set; }
    public bool GerouProposta { get; set; }
}

public class ConsentimentoLgpdRequest
{
    public bool Aceitou { get; set; }
}

public class ClienteLgpdExport
{
    public ClienteResponse Cliente { get; set; } = new();
    public List<InteracaoResponse> Interacoes { get; set; } = new();
    public List<VisitaResponse> Visitas { get; set; } = new();
    public List<object> Vendas { get; set; } = new();
    public List<object> Reservas { get; set; } = new();
    public DateTime ExportadoEm { get; set; } = DateTime.UtcNow;
}
