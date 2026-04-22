using ImovelStand.Application.Abstractions;
using ImovelStand.Infrastructure.Interceptors;
using ImovelStand.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ImovelStand.Tests.Fakes;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create(Guid? tenantId = null, bool withInterceptors = false)
    {
        var provider = new TestTenantProvider(tenantId ?? Guid.NewGuid());
        return Create(provider, withInterceptors);
    }

    public static ApplicationDbContext Create(ITenantProvider provider, bool withInterceptors = false)
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        if (withInterceptors)
        {
            builder.AddInterceptors(
                new HistoricoPrecoInterceptor(),
                new TenantAssignmentInterceptor(provider));
        }

        return new ApplicationDbContext(builder.Options, provider);
    }
}
