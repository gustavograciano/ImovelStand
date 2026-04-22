namespace ImovelStand.Domain.Abstractions;

/// <summary>
/// Marca uma entidade como pertencente a um tenant (isolamento multi-tenant).
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
