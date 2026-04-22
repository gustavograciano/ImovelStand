using System.Security.Claims;
using ImovelStand.Application.Abstractions;

namespace ImovelStand.Api.Services;

public class HttpTenantProvider : ITenantProvider
{
    public const string TenantClaimType = "tenantId";

    private static readonly AsyncLocal<Guid?> _override = new();
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            if (_override.Value is { } scoped) return scoped;

            var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(TenantClaimType);
            return Guid.TryParse(claim, out var tenantId) ? tenantId : Guid.Empty;
        }
    }

    public bool HasTenant => TenantId != Guid.Empty;

    public IDisposable BeginScope(Guid tenantId)
    {
        var previous = _override.Value;
        _override.Value = tenantId;
        return new ScopeReset(previous);
    }

    private sealed class ScopeReset : IDisposable
    {
        private readonly Guid? _previous;
        public ScopeReset(Guid? previous) => _previous = previous;
        public void Dispose() => _override.Value = _previous;
    }
}
