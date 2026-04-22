using ImovelStand.Application.Abstractions;
using ImovelStand.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImovelStand.Jobs.Jobs;

public class LembreteReservaVencendoJob
{
    private readonly ApplicationDbContext _context;
    private readonly INotificador _notificador;
    private readonly ILogger<LembreteReservaVencendoJob> _logger;

    public LembreteReservaVencendoJob(ApplicationDbContext context, INotificador notificador, ILogger<LembreteReservaVencendoJob> logger)
    {
        _context = context;
        _notificador = notificador;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var agora = DateTime.UtcNow;
        var daquiA24h = agora.AddHours(24);

        var reservas = await _context.Reservas
            .IgnoreQueryFilters()
            .Include(r => r.Cliente)
            .Include(r => r.Apartamento)
            .Where(r => r.Status == "Ativa"
                && r.DataExpiracao != null
                && r.DataExpiracao > agora
                && r.DataExpiracao <= daquiA24h)
            .ToListAsync(cancellationToken);

        foreach (var r in reservas)
        {
            if (string.IsNullOrWhiteSpace(r.Cliente?.Email)) continue;

            await _notificador.EnviarEmailAsync(
                r.Cliente.Email,
                "Sua reserva vence em 24 horas",
                $"<p>Olá {r.Cliente.Nome},</p><p>Sua reserva do apartamento {r.Apartamento?.Numero} vence em menos de 24 horas. Entre em contato com seu corretor para seguir.</p>",
                cancellationToken);
        }

        _logger.LogInformation("{Qtd} lembretes de reserva enviados.", reservas.Count);
    }
}
