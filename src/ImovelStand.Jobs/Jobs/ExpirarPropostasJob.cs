using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImovelStand.Jobs.Jobs;

public class ExpirarPropostasJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExpirarPropostasJob> _logger;

    public ExpirarPropostasJob(ApplicationDbContext context, ILogger<ExpirarPropostasJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var agora = DateTime.UtcNow;
        var propostas = await _context.Propostas
            .IgnoreQueryFilters()
            .Where(p => p.DataValidade != null
                && p.DataValidade < agora
                && (p.Status == StatusProposta.Enviada
                    || p.Status == StatusProposta.ContrapropostaCliente
                    || p.Status == StatusProposta.ContrapropostaCorretor))
            .ToListAsync(cancellationToken);

        if (propostas.Count == 0) return;

        _logger.LogInformation("Expirando {Qtd} propostas.", propostas.Count);

        foreach (var p in propostas)
        {
            var anterior = p.Status;
            p.Status = StatusProposta.Expirada;
            p.UpdatedAt = agora;
            _context.PropostaHistoricos.Add(new PropostaHistoricoStatus
            {
                TenantId = p.TenantId,
                PropostaId = p.Id,
                StatusAnterior = anterior,
                StatusNovo = StatusProposta.Expirada,
                Motivo = "Expirada automaticamente (DataValidade vencida).",
                DataAlteracao = agora
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
