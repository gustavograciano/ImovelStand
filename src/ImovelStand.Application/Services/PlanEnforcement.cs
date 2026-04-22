using ImovelStand.Domain.Entities;

namespace ImovelStand.Application.Services;

public static class PlanEnforcement
{
    public static bool AssinaturaAtiva(Assinatura? assinatura) =>
        assinatura?.EstaAtiva ?? false;

    public static bool ExcedeLimite(Plano plano, string limite, int valorAtual) =>
        limite switch
        {
            "empreendimentos" => valorAtual >= plano.MaxEmpreendimentos,
            "unidades" => valorAtual >= plano.MaxUnidades,
            "usuarios" => valorAtual >= plano.MaxUsuarios,
            _ => false
        };
}
