using ImovelStand.Application.Abstractions;
using ImovelStand.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ImovelStand.Infrastructure.Interceptors;

/// <summary>
/// Auto-atribui <c>TenantId</c> em entidades <see cref="ITenantEntity"/> recém-criadas
/// que vieram do controller sem TenantId setado. Evita que o desenvolvedor esqueça e
/// crie registros órfãos ou (pior) vazando pra outro tenant.
/// </summary>
public class TenantAssignmentInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;

    public TenantAssignmentInterceptor(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Assign(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Assign(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Assign(DbContext? context)
    {
        if (context is null || !_tenantProvider.HasTenant) return;

        foreach (var entry in context.ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            {
                entry.Entity.TenantId = _tenantProvider.TenantId;
            }
        }
    }
}
