using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ImovelStand.Application.Abstractions;
using ImovelStand.Application.Services;
using ImovelStand.Domain.Entities;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Authorization;

/// <summary>
/// Bloqueia o endpoint se a assinatura do tenant não está ativa ou se o plano não
/// cobre o recurso solicitado. Opcionalmente valida limite (ex: <c>MaxEmpreendimentos</c>).
/// </summary>
/// <example>
/// [RequiresPlan("Pro")]                          // exige plano Pro ou superior
/// [RequiresPlan(limit: "empreendimentos")]       // valida limite de empreendimentos
/// </example>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequiresPlanAttribute : Attribute, IAsyncAuthorizationFilter
{
    private static readonly string[] PlanoOrder = { "Starter", "Pro", "Business" };

    public string? PlanoMinimo { get; }
    public string? Limit { get; }

    public RequiresPlanAttribute(string? plano = null, string? limit = null)
    {
        PlanoMinimo = plano;
        Limit = limit;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var tenantProvider = context.HttpContext.RequestServices.GetRequiredService<ITenantProvider>();
        if (!tenantProvider.HasTenant)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

        var assinatura = await db.Set<Assinatura>().IgnoreQueryFilters()
            .Include(a => a.Plano)
            .Where(a => a.TenantId == tenantProvider.TenantId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();

        if (!PlanEnforcement.AssinaturaAtiva(assinatura))
        {
            context.Result = new ObjectResult(new
            {
                type = "https://imovelstand.com.br/errors/plan-required",
                title = "Assinatura inativa",
                status = 402,
                detail = "Sua assinatura está inativa, em atraso ou cancelada. Regularize para continuar."
            })
            { StatusCode = 402 };
            return;
        }

        if (!string.IsNullOrWhiteSpace(PlanoMinimo))
        {
            var atualIdx = Array.IndexOf(PlanoOrder, assinatura!.Plano.Nome);
            var minimoIdx = Array.IndexOf(PlanoOrder, PlanoMinimo);
            if (atualIdx < minimoIdx)
            {
                context.Result = new ObjectResult(new
                {
                    type = "https://imovelstand.com.br/errors/plan-upgrade-required",
                    title = "Upgrade de plano necessário",
                    status = 402,
                    detail = $"Este recurso requer plano {PlanoMinimo} ou superior. Seu plano atual é {assinatura.Plano.Nome}.",
                    planoAtual = assinatura.Plano.Nome,
                    planoNecessario = PlanoMinimo
                })
                { StatusCode = 402 };
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(Limit))
        {
            var atual = Limit switch
            {
                "empreendimentos" => await db.Empreendimentos.CountAsync(),
                "unidades" => await db.Apartamentos.CountAsync(),
                "usuarios" => await db.Usuarios.CountAsync(),
                _ => 0
            };
            if (PlanEnforcement.ExcedeLimite(assinatura!.Plano, Limit, atual))
            {
                context.Result = new ObjectResult(new
                {
                    type = "https://imovelstand.com.br/errors/plan-limit-reached",
                    title = "Limite do plano atingido",
                    status = 402,
                    detail = $"Você atingiu o limite de {Limit} do plano {assinatura.Plano.Nome}. Faça upgrade ou remova registros existentes.",
                    limit = Limit,
                    valorAtual = atual
                })
                { StatusCode = 402 };
            }
        }
    }
}
