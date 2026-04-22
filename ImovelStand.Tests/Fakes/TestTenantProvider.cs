using ImovelStand.Application.Abstractions;

namespace ImovelStand.Tests.Fakes;

public class TestTenantProvider : ITenantProvider
{
    public TestTenantProvider(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public Guid TenantId { get; private set; }

    public bool HasTenant => TenantId != Guid.Empty;

    public IDisposable BeginScope(Guid tenantId)
    {
        var previous = TenantId;
        TenantId = tenantId;
        return new Reset(() => TenantId = previous);
    }

    private sealed class Reset : IDisposable
    {
        private readonly Action _action;
        public Reset(Action action) => _action = action;
        public void Dispose() => _action();
    }
}
