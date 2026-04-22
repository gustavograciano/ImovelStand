using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;
using ImovelStand.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace ImovelStand.Tests.Persistence;

/// <summary>
/// Garante que o <c>HasQueryFilter</c> global isola dados entre tenants.
/// Fosse um vazamento cross-tenant = fim do negócio; estes testes fecham essa porta.
/// </summary>
public class MultiTenantIsolationTests
{
    private static readonly Guid TenantA = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TenantB = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static string SharedDbName() => $"MultiTenantDb_{Guid.NewGuid()}";

    private static ApplicationDbContext Create(Guid tenantId, string dbName)
    {
        var provider = new TestTenantProvider(tenantId);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ApplicationDbContext(options, provider);
    }

    private static async Task SeedCrossTenantAsync(string dbName)
    {
        // Tenant A vê só o cliente A; Tenant B só o cliente B. Mesma base, filter separa.
        using var ctxA = Create(TenantA, dbName);
        ctxA.Clientes.Add(new Cliente
        {
            Id = 1,
            TenantId = TenantA,
            Nome = "Cliente do A",
            Cpf = "11111111111",
            Email = "a@a.com",
            Telefone = "11"
        });
        await ctxA.SaveChangesAsync();

        using var ctxB = Create(TenantB, dbName);
        ctxB.Clientes.Add(new Cliente
        {
            Id = 2,
            TenantId = TenantB,
            Nome = "Cliente do B",
            Cpf = "22222222222",
            Email = "b@b.com",
            Telefone = "22"
        });
        await ctxB.SaveChangesAsync();
    }

    [Fact]
    public async Task List_NaoDeveRetornarDadosDeOutroTenant()
    {
        var db = SharedDbName();
        await SeedCrossTenantAsync(db);

        using var ctxA = Create(TenantA, db);
        var clientesA = await ctxA.Clientes.ToListAsync();

        Assert.Single(clientesA);
        Assert.Equal("Cliente do A", clientesA[0].Nome);
    }

    [Fact]
    public async Task Get_NaoDeveEncontrarRegistroDeOutroTenant()
    {
        var db = SharedDbName();
        await SeedCrossTenantAsync(db);

        using var ctxA = Create(TenantA, db);
        // Tenta pegar o cliente do B (Id=2) estando autenticado como A
        var clienteB = await ctxA.Clientes.FirstOrDefaultAsync(c => c.Id == 2);

        Assert.Null(clienteB);
    }

    [Fact]
    public async Task Count_RefleteApenasTenantAtual()
    {
        var db = SharedDbName();
        await SeedCrossTenantAsync(db);

        using var ctxA = Create(TenantA, db);
        using var ctxB = Create(TenantB, db);

        Assert.Equal(1, await ctxA.Clientes.CountAsync());
        Assert.Equal(1, await ctxB.Clientes.CountAsync());
    }

    [Fact]
    public async Task IgnoreQueryFilters_DeveEnxergarTodosOsTenants()
    {
        var db = SharedDbName();
        await SeedCrossTenantAsync(db);

        using var ctxA = Create(TenantA, db);
        // Escape hatch para admin/jobs: IgnoreQueryFilters ignora o filtro multi-tenant.
        var todos = await ctxA.Clientes.IgnoreQueryFilters().ToListAsync();

        Assert.Equal(2, todos.Count);
    }

    [Fact]
    public async Task InterceptorDeveAutoAtribuirTenantIdEmRegistrosNovos()
    {
        var provider = new TestTenantProvider(TenantA);
        using var ctx = TestDbContextFactory.Create(provider, withInterceptors: true);

        // Cliente criado sem TenantId explícito — interceptor deve preencher.
        var cliente = new Cliente
        {
            Nome = "Novo",
            Cpf = "33333333333",
            Email = "n@n.com",
            Telefone = "33"
        };
        ctx.Clientes.Add(cliente);
        await ctx.SaveChangesAsync();

        Assert.Equal(TenantA, cliente.TenantId);
    }

    [Fact]
    public async Task InterceptorNaoDeveSobrescreverTenantIdExplicito()
    {
        var provider = new TestTenantProvider(TenantA);
        using var ctx = TestDbContextFactory.Create(provider, withInterceptors: true);

        // Caso edge: admin/seed seta TenantId manualmente; interceptor não sobrescreve.
        var cliente = new Cliente
        {
            TenantId = TenantB,
            Nome = "Forcado",
            Cpf = "44444444444",
            Email = "f@f.com",
            Telefone = "44"
        };
        ctx.Clientes.Add(cliente);
        await ctx.SaveChangesAsync();

        Assert.Equal(TenantB, cliente.TenantId);
    }
}
