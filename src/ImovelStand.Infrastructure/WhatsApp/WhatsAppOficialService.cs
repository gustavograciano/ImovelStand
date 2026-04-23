using System.Text.Json;
using ImovelStand.Application.Abstractions;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImovelStand.Infrastructure.WhatsApp;

/// <summary>
/// Orquestra envio via WhatsApp oficial:
/// - Resolve template a partir do ID
/// - Chama IWhatsAppOficialProvider (Meta Cloud API)
/// - Persiste WhatsAppMensagem
/// - Cria HistoricoInteracao do cliente
/// - Atualiza status em callbacks futuros (Sprint 29.2)
/// </summary>
public class WhatsAppOficialService
{
    private readonly IWhatsAppOficialProvider _provider;
    private readonly ApplicationDbContext _context;
    private readonly WhatsAppOficialOptions _options;
    private readonly ILogger<WhatsAppOficialService> _logger;

    public WhatsAppOficialService(
        IWhatsAppOficialProvider provider,
        ApplicationDbContext context,
        IOptions<WhatsAppOficialOptions> options,
        ILogger<WhatsAppOficialService> logger)
    {
        _provider = provider;
        _context = context;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<WhatsAppMensagem> EnviarTemplateParaClienteAsync(
        int clienteId,
        int templateId,
        List<string> variaveis,
        int? usuarioId,
        CancellationToken ct = default)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId, ct)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        var template = await _context.WhatsAppTemplates.FirstOrDefaultAsync(t => t.Id == templateId, ct)
            ?? throw new InvalidOperationException("Template não encontrado.");

        if (!template.Ativo)
            throw new InvalidOperationException("Template inativo.");

        var numeroDest = !string.IsNullOrWhiteSpace(cliente.Whatsapp) ? cliente.Whatsapp : cliente.Telefone;
        if (string.IsNullOrWhiteSpace(numeroDest))
            throw new InvalidOperationException("Cliente não tem telefone nem WhatsApp cadastrado.");

        // Monta conteudo final para visualização local
        var conteudoFinal = template.Corpo;
        for (var i = 0; i < variaveis.Count; i++)
        {
            conteudoFinal = conteudoFinal.Replace($"{{{{{i + 1}}}}}", variaveis[i]);
        }

        var msg = new WhatsAppMensagem
        {
            TenantId = cliente.TenantId,
            ClienteId = cliente.Id,
            UsuarioId = usuarioId,
            TelefoneContato = numeroDest,
            NumeroEmpresa = _options.PhoneNumberId,
            Direcao = DirecaoWhatsApp.Enviada,
            Status = StatusMensagemWhatsApp.Pendente,
            TemplateId = template.Id,
            VariaveisJson = JsonSerializer.Serialize(variaveis),
            Conteudo = conteudoFinal,
            CreatedAt = DateTime.UtcNow
        };
        _context.WhatsAppMensagens.Add(msg);
        await _context.SaveChangesAsync(ct);

        var resultado = await _provider.EnviarTemplateAsync(new EnvioTemplateRequest
        {
            NumeroDestino = numeroDest,
            NumeroRemetente = _options.PhoneNumberId,
            NomeTemplate = template.Nome,
            Idioma = template.Idioma,
            Variaveis = variaveis
        }, ct);

        if (resultado.Sucesso)
        {
            msg.Status = StatusMensagemWhatsApp.Aceita;
            msg.EnviadaEm = DateTime.UtcNow;
            msg.ProviderMessageId = resultado.ProviderMessageId;

            // Cria HistoricoInteracao vinculado
            _context.HistoricoInteracoes.Add(new HistoricoInteracao
            {
                TenantId = cliente.TenantId,
                ClienteId = cliente.Id,
                UsuarioId = usuarioId,
                Tipo = TipoInteracao.Whatsapp,
                Conteudo = $"[Template {template.Nome}] {conteudoFinal}",
                DataHora = DateTime.UtcNow
            });
        }
        else
        {
            msg.Status = StatusMensagemWhatsApp.Falhou;
            msg.MensagemErro = resultado.MensagemErro;
            _logger.LogWarning("WhatsApp envio falhou para cliente {Id}: {Err}", clienteId, resultado.MensagemErro);
        }

        await _context.SaveChangesAsync(ct);
        return msg;
    }

    public async Task<WhatsAppMensagem> EnviarTextoLivreAsync(
        int clienteId,
        string texto,
        int? usuarioId,
        CancellationToken ct = default)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId, ct)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        // Verifica janela de sessão 24h: deve haver mensagem RECEBIDA do cliente
        // nas últimas 24h para Meta permitir texto livre
        var limite = DateTime.UtcNow.AddHours(-24);
        var dentroJanela = await _context.WhatsAppMensagens.AnyAsync(m =>
            m.ClienteId == clienteId
            && m.Direcao == DirecaoWhatsApp.Recebida
            && m.CreatedAt >= limite, ct);

        if (!dentroJanela && !_options.ModoStub)
        {
            throw new InvalidOperationException(
                "Fora da janela de sessão 24h. Use um template aprovado.");
        }

        var numeroDest = !string.IsNullOrWhiteSpace(cliente.Whatsapp) ? cliente.Whatsapp : cliente.Telefone;
        if (string.IsNullOrWhiteSpace(numeroDest))
            throw new InvalidOperationException("Cliente sem telefone.");

        var msg = new WhatsAppMensagem
        {
            TenantId = cliente.TenantId,
            ClienteId = cliente.Id,
            UsuarioId = usuarioId,
            TelefoneContato = numeroDest,
            NumeroEmpresa = _options.PhoneNumberId,
            Direcao = DirecaoWhatsApp.Enviada,
            Status = StatusMensagemWhatsApp.Pendente,
            Conteudo = texto,
            CreatedAt = DateTime.UtcNow
        };
        _context.WhatsAppMensagens.Add(msg);
        await _context.SaveChangesAsync(ct);

        var resultado = await _provider.EnviarTextoAsync(new EnvioTextoRequest
        {
            NumeroDestino = numeroDest,
            NumeroRemetente = _options.PhoneNumberId,
            Texto = texto
        }, ct);

        if (resultado.Sucesso)
        {
            msg.Status = StatusMensagemWhatsApp.Aceita;
            msg.EnviadaEm = DateTime.UtcNow;
            msg.ProviderMessageId = resultado.ProviderMessageId;

            _context.HistoricoInteracoes.Add(new HistoricoInteracao
            {
                TenantId = cliente.TenantId,
                ClienteId = cliente.Id,
                UsuarioId = usuarioId,
                Tipo = TipoInteracao.Whatsapp,
                Conteudo = texto,
                DataHora = DateTime.UtcNow
            });
        }
        else
        {
            msg.Status = StatusMensagemWhatsApp.Falhou;
            msg.MensagemErro = resultado.MensagemErro;
        }

        await _context.SaveChangesAsync(ct);
        return msg;
    }

    public async Task<List<WhatsAppMensagem>> ListarMensagensAsync(int clienteId, CancellationToken ct = default)
    {
        return await _context.WhatsAppMensagens
            .AsNoTracking()
            .Where(m => m.ClienteId == clienteId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(100)
            .ToListAsync(ct);
    }
}
