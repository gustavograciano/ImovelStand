namespace ImovelStand.Application.Dtos;

public class UsuarioCreateRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Role { get; set; } = "Corretor";
    public string? Creci { get; set; }
    public decimal? PercentualComissao { get; set; }
}

public class UsuarioUpdateRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Role { get; set; } = "Corretor";
    public string? Creci { get; set; }
    public decimal? PercentualComissao { get; set; }
    public bool Ativo { get; set; } = true;
}

public class TrocarSenhaRequest
{
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}

public class UsuarioResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Creci { get; set; }
    public decimal? PercentualComissao { get; set; }
    public bool Ativo { get; set; }
    public DateTime? UltimoLoginEm { get; set; }
    public DateTime DataCadastro { get; set; }
}
