using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;
using ImovelStand.Domain.Enums;
using ImovelStand.Domain.ValueObjects;

namespace ImovelStand.Domain.Entities;

public class Cliente : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(14)]
    public string Cpf { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Rg { get; set; }

    public DateTime? DataNascimento { get; set; }

    public EstadoCivil? EstadoCivil { get; set; }

    public RegimeBens? RegimeBens { get; set; }

    [MaxLength(100)]
    public string? Profissao { get; set; }

    [MaxLength(200)]
    public string? Empresa { get; set; }

    public decimal? RendaMensal { get; set; }

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(15)]
    public string Telefone { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? Whatsapp { get; set; }

    public Endereco? Endereco { get; set; }

    public OrigemLead? OrigemLead { get; set; }

    public StatusFunil StatusFunil { get; set; } = StatusFunil.Lead;

    /// <summary>Aceita receber email? Respeitado pelos jobs de notificação.</summary>
    public bool AceitaEmail { get; set; } = true;

    /// <summary>Aceita receber WhatsApp? Respeitado pelos jobs de notificação.</summary>
    public bool AceitaWhatsapp { get; set; } = true;

    /// <summary>Aceita receber SMS? (feature futura)</summary>
    public bool AceitaSms { get; set; } = false;

    public int? CorretorResponsavelId { get; set; }

    public int? ConjugeId { get; set; }

    /// <summary>LGPD Art. 8: consentimento explícito para uso de dados pessoais.</summary>
    public bool ConsentimentoLgpd { get; set; }

    public DateTime? ConsentimentoLgpdEm { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CorretorResponsavelId))]
    public virtual Usuario? CorretorResponsavel { get; set; }

    [ForeignKey(nameof(ConjugeId))]
    public virtual Cliente? Conjuge { get; set; }

    public virtual ICollection<ClienteDependente> Dependentes { get; set; } = new List<ClienteDependente>();
    public virtual ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public virtual ICollection<HistoricoInteracao> Interacoes { get; set; } = new List<HistoricoInteracao>();
    public virtual ICollection<Visita> Visitas { get; set; } = new List<Visita>();
}
