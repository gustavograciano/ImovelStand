using ImovelStand.Domain.Enums;

namespace ImovelStand.Application.Dtos;

public class CondicaoPagamentoDto
{
    public decimal ValorTotal { get; set; }
    public decimal Entrada { get; set; }
    public DateTime? EntradaData { get; set; }
    public decimal Sinal { get; set; }
    public DateTime? SinalData { get; set; }
    public int QtdParcelasMensais { get; set; }
    public decimal ValorParcelaMensal { get; set; }
    public DateTime? PrimeiraParcelaData { get; set; }
    public int QtdSemestrais { get; set; }
    public decimal ValorSemestral { get; set; }
    public decimal ValorChaves { get; set; }
    public DateTime? ChavesDataPrevista { get; set; }
    public int QtdPosChaves { get; set; }
    public decimal ValorPosChaves { get; set; }
    public IndiceReajuste Indice { get; set; }
    public decimal TaxaJurosAnual { get; set; }
}

public class PropostaCreateRequest
{
    public int ClienteId { get; set; }
    public int ApartamentoId { get; set; }
    public int CorretorId { get; set; }
    public decimal ValorOferecido { get; set; }
    public DateTime? DataValidade { get; set; }
    public string? Observacoes { get; set; }
    public CondicaoPagamentoDto Condicao { get; set; } = new();
}

public class PropostaResponse
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string? ClienteNome { get; set; }
    public int ApartamentoId { get; set; }
    public string? ApartamentoNumero { get; set; }
    public int CorretorId { get; set; }
    public string? CorretorNome { get; set; }
    public int Versao { get; set; }
    public int? PropostaOriginalId { get; set; }
    public decimal ValorOferecido { get; set; }
    public StatusProposta Status { get; set; }
    public DateTime? DataEnvio { get; set; }
    public DateTime? DataValidade { get; set; }
    public DateTime? DataRespostaCliente { get; set; }
    public string? Observacoes { get; set; }
    public CondicaoPagamentoDto Condicao { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class ContrapropostaRequest
{
    public decimal ValorOferecido { get; set; }
    public string? Observacoes { get; set; }
    public CondicaoPagamentoDto Condicao { get; set; } = new();
    /// <summary>Se true, é contraproposta do corretor; senão, do cliente.</summary>
    public bool VemDoCorretor { get; set; }
}

public class AlterarStatusRequest
{
    public StatusProposta NovoStatus { get; set; }
    public string? Motivo { get; set; }
}
