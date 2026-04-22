namespace ImovelStand.Application.Abstractions;

/// <summary>
/// Expõe o TenantId da request atual para servicos scoped (DbContext, controllers, etc).
/// Implementado na Api a partir das claims do JWT.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// TenantId da request atual. Retorna Guid.Empty quando não há request (jobs) ou
    /// o usuário não está autenticado — o DbContext pula o filtro nesse caso.
    /// </summary>
    Guid TenantId { get; }

    bool HasTenant { get; }

    /// <summary>
    /// Usado por jobs/tests para rodar fora do escopo de uma request HTTP.
    /// </summary>
    IDisposable BeginScope(Guid tenantId);
}
