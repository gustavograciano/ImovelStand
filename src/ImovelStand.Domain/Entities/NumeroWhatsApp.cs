using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImovelStand.Domain.Abstractions;

namespace ImovelStand.Domain.Entities;

/// <summary>
/// Número verificado no Meta Cloud API vinculado a um corretor específico.
/// Permite que incorporadora tenha N números (1 por corretor) e roteie
/// mensagens corretamente para cada.
/// </summary>
public class NumeroWhatsApp : ITenantEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// Phone Number ID do Meta (não é o número em si — é o ID interno).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PhoneNumberId { get; set; } = string.Empty;

    /// <summary>
    /// Número em formato E.164 exibido ao cliente (ex: +5511999998888).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string NumeroExibicao { get; set; } = string.Empty;

    /// <summary>
    /// Apelido do número (ex: "Corretor João - Vendas"). Ajuda na gestão.
    /// </summary>
    [MaxLength(100)]
    public string? Apelido { get; set; }

    /// <summary>
    /// Corretor responsável pelo número. Null = número compartilhado
    /// (ex: número geral da incorporadora, distribuído round-robin).
    /// </summary>
    public int? UsuarioId { get; set; }

    /// <summary>
    /// Se compartilhado (UsuarioId null), indica ordem preferencial para
    /// round-robin. Evita sempre cair no mesmo corretor.
    /// </summary>
    public int OrdemRoundRobin { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UsuarioId))]
    public virtual Usuario? Usuario { get; set; }
}
