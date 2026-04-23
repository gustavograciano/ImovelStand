namespace ImovelStand.Application.Dtos;

public class TorreCreateRequest
{
    public int EmpreendimentoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Pavimentos { get; set; }
    public int ApartamentosPorPavimento { get; set; }
}

public class TorreUpdateRequest
{
    public string Nome { get; set; } = string.Empty;
    public int Pavimentos { get; set; }
    public int ApartamentosPorPavimento { get; set; }
}

public class TorreResponse
{
    public int Id { get; set; }
    public int EmpreendimentoId { get; set; }
    public string? EmpreendimentoNome { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Pavimentos { get; set; }
    public int ApartamentosPorPavimento { get; set; }
    public int QtdApartamentos { get; set; }
}

public class TipologiaCreateRequest
{
    public int EmpreendimentoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal AreaPrivativa { get; set; }
    public decimal AreaTotal { get; set; }
    public int Quartos { get; set; }
    public int Suites { get; set; }
    public int Banheiros { get; set; }
    public int Vagas { get; set; }
    public decimal PrecoBase { get; set; }
    public string? PlantaUrl { get; set; }
}

public class TipologiaUpdateRequest : TipologiaCreateRequest { }

public class TipologiaResponse
{
    public int Id { get; set; }
    public int EmpreendimentoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal AreaPrivativa { get; set; }
    public decimal AreaTotal { get; set; }
    public int Quartos { get; set; }
    public int Suites { get; set; }
    public int Banheiros { get; set; }
    public int Vagas { get; set; }
    public decimal PrecoBase { get; set; }
    public string? PlantaUrl { get; set; }
}
