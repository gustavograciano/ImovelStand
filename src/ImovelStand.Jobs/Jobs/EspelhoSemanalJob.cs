using ImovelStand.Application.Abstractions;
using ImovelStand.Application.Services;
using ImovelStand.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImovelStand.Jobs.Jobs;

/// <summary>
/// Gera o espelho executivo de cada empreendimento e envia pro email dos
/// Admins do tenant. Roda sexta-feira 18h (hora local do servidor) via
/// Hangfire RecurringJob.
/// </summary>
public class EspelhoSemanalJob
{
    private readonly ApplicationDbContext _context;
    private readonly EspelhoPdfGenerator _generator;
    private readonly INotificador _notificador;
    private readonly ILogger<EspelhoSemanalJob> _logger;

    public EspelhoSemanalJob(
        ApplicationDbContext context,
        EspelhoPdfGenerator generator,
        INotificador notificador,
        ILogger<EspelhoSemanalJob> logger)
    {
        _context = context;
        _generator = generator;
        _notificador = notificador;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // IgnoreQueryFilters: job sem tenant scope — vamos iterar todos os tenants.
        var tenants = await _context.Tenants.IgnoreQueryFilters()
            .Where(t => t.Ativo)
            .ToListAsync(cancellationToken);

        foreach (var tenant in tenants)
        {
            var empreendimentos = await _context.Empreendimentos.IgnoreQueryFilters()
                .Where(e => e.TenantId == tenant.Id)
                .ToListAsync(cancellationToken);

            var adminEmails = await _context.Usuarios.IgnoreQueryFilters()
                .Where(u => u.TenantId == tenant.Id && u.Ativo && u.Role == "Admin")
                .Select(u => u.Email)
                .ToListAsync(cancellationToken);

            if (adminEmails.Count == 0)
            {
                _logger.LogInformation("Tenant {Tenant} sem admins ativos; espelho não enviado.", tenant.Slug);
                continue;
            }

            foreach (var emp in empreendimentos)
            {
                var torres = await _context.Torres.IgnoreQueryFilters()
                    .Where(t => t.EmpreendimentoId == emp.Id).ToListAsync(cancellationToken);
                var tipologias = await _context.Tipologias.IgnoreQueryFilters()
                    .Where(t => t.EmpreendimentoId == emp.Id).ToListAsync(cancellationToken);
                var torreIds = torres.Select(t => t.Id).ToArray();
                var apartamentos = await _context.Apartamentos.IgnoreQueryFilters()
                    .Where(a => torreIds.Contains(a.TorreId)).ToListAsync(cancellationToken);

                var metadata = new EspelhoMetadata(tenant.Nome, "Sistema (job semanal)", DateTime.UtcNow);
                var pdf = _generator.Gerar(TipoEspelho.Executivo, emp, torres, tipologias, apartamentos, metadata);

                foreach (var email in adminEmails)
                {
                    // Anexo via email real depende da extensão do MailKitNotificador;
                    // aqui mandamos só a notificação com resumo + link de geração sob demanda.
                    var html = $"""
                        <p>Espelho executivo semanal de <b>{emp.Nome}</b>:</p>
                        <ul>
                          <li>Unidades totais: {apartamentos.Count}</li>
                          <li>Vendidas: {apartamentos.Count(a => a.Status == Domain.Enums.StatusApartamento.Vendido)}</li>
                          <li>VGV total: R$ {apartamentos.Sum(a => a.PrecoAtual):N2}</li>
                        </ul>
                        <p>Baixe o PDF completo em <code>GET /api/empreendimentos/{emp.Id}/espelho?tipo=Executivo</code>.</p>
                        """;
                    await _notificador.EnviarEmailAsync(email, $"[ImovelStand] Espelho semanal — {emp.Nome}", html, cancellationToken);
                }

                _logger.LogInformation("Espelho semanal enviado: tenant={Tenant} emp={Emp} admins={Count} bytes={Bytes}",
                    tenant.Slug, emp.Nome, adminEmails.Count, pdf.Length);
            }
        }
    }
}
