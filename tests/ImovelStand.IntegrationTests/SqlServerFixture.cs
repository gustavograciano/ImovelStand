using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using ImovelStand.Application.Abstractions;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.IntegrationTests;

/// <summary>
/// Fixture compartilhada: sobe um container SQL Server uma vez por assembly
/// e roda migrations. Testes rodam contra DB real, não InMemory.
/// </summary>
public class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container;
    public string ConnectionString => _container.GetConnectionString();

    public SqlServerFixture()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        using var ctx = CreateContext(new TestTenantProvider(Guid.NewGuid()));
        await ctx.Database.MigrateAsync();
    }

    public ApplicationDbContext CreateContext(ITenantProvider tenantProvider)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new ApplicationDbContext(options, tenantProvider);
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}

public class TestTenantProvider : ITenantProvider
{
    public TestTenantProvider(Guid tenantId) { TenantId = tenantId; }
    public Guid TenantId { get; private set; }
    public bool HasTenant => TenantId != Guid.Empty;
    public IDisposable BeginScope(Guid tenantId)
    {
        var prev = TenantId;
        TenantId = tenantId;
        return new Reset(() => TenantId = prev);
    }
    private sealed class Reset : IDisposable
    {
        private readonly Action _action;
        public Reset(Action action) => _action = action;
        public void Dispose() => _action();
    }
}

[CollectionDefinition("SqlServer")]
public class SqlServerCollection : ICollectionFixture<SqlServerFixture> { }
