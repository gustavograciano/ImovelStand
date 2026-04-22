using ImovelStand.Application.Abstractions;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImovelStand.Jobs.Jobs;

/// <summary>
/// Expira reservas vencidas (DataExpiracao &lt; now), libera apartamento, notifica corretor.
/// Roda a cada 10min via Hangfire RecurringJob.
/// </summary>
public class ExpirarReservasJob
{
    private readonly ApplicationDbContext _context;
    private readonly INotificador _notificador;
    private readonly ILogger<ExpirarReservasJob> _logger;

    public ExpirarReservasJob(ApplicationDbContext context, INotificador notificador, ILogger<ExpirarReservasJob> logger)
    {
        _context = context;
        _notificador = notificador;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var agora = DateTime.UtcNow;
        var reservas = await _context.Reservas
            .IgnoreQueryFilters()
            .Include(r => r.Cliente)
            .Include(r => r.Apartamento)
            .Where(r => r.Status == "Ativa" && r.DataExpiracao != null && r.DataExpiracao < agora)
            .ToListAsync(cancellationToken);

        if (reservas.Count == 0) return;

        _logger.LogInformation("Expirando {Qtd} reservas.", reservas.Count);

        foreach (var reserva in reservas)
        {
            reserva.Status = "Expirada";
            if (reserva.Apartamento?.Status == StatusApartamento.Reservado)
                reserva.Apartamento.Status = StatusApartamento.Disponivel;

            if (!string.IsNullOrWhiteSpace(reserva.Cliente?.Email))
            {
                await _notificador.EnviarEmailAsync(
                    reserva.Cliente.Email,
                    "Sua reserva expirou",
                    $"<p>Olá {reserva.Cliente.Nome},</p><p>Sua reserva do apartamento {reserva.Apartamento?.Numero} expirou. Se ainda tiver interesse, fale com seu corretor.</p>",
                    cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
