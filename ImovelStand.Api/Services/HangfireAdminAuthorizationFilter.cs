using Hangfire.Dashboard;

namespace ImovelStand.Api.Services;

/// <summary>
/// Filtro de autorização do Hangfire dashboard. Em desenvolvimento, libera tudo.
/// Em produção, exige usuário autenticado com role Admin.
/// </summary>
public class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IHostEnvironment _env;

    public HangfireAdminAuthorizationFilter(IHostEnvironment env)
    {
        _env = env;
    }

    public bool Authorize(DashboardContext context)
    {
        if (_env.IsDevelopment()) return true;
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("Admin");
    }
}
