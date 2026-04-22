using ImovelStand.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ImovelStand.Infrastructure.Interceptors;

public class HistoricoPrecoInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Capture(DbContext? context)
    {
        if (context is null) return;

        var entries = context.ChangeTracker
            .Entries<Apartamento>()
            .Where(e => e.State == EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
        {
            var precoProp = entry.Property(nameof(Apartamento.PrecoAtual));
            if (!precoProp.IsModified) continue;

            var precoAnterior = (decimal)(precoProp.OriginalValue ?? 0m);
            var precoNovo = (decimal)(precoProp.CurrentValue ?? 0m);
            if (precoAnterior == precoNovo) continue;

            context.Add(new HistoricoPreco
            {
                ApartamentoId = entry.Entity.Id,
                PrecoAnterior = precoAnterior,
                PrecoNovo = precoNovo,
                DataAlteracao = DateTime.UtcNow
            });
        }
    }
}
