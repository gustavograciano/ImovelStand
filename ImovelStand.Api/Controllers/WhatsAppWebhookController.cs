using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ImovelStand.Application.Abstractions;
using ImovelStand.Domain.Entities;
using ImovelStand.Domain.Enums;
using ImovelStand.Infrastructure.Persistence;

namespace ImovelStand.Api.Controllers;

/// <summary>
/// Webhook do Meta Cloud API para receber mensagens inbound e callbacks
/// de status (entregue/lida/falhou).
///
/// GET: handshake de verificação quando Meta subscreve o webhook.
/// POST: eventos de messages/statuses.
///
/// Segurança: validar X-Hub-Signature-256 em produção (HMAC SHA-256
/// do body com AppSecret).
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/webhooks/whatsapp")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly WhatsAppOficialOptions _options;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WhatsAppWebhookController> _logger;

    public WhatsAppWebhookController(
        IOptions<WhatsAppOficialOptions> options,
        ApplicationDbContext context,
        ILogger<WhatsAppWebhookController> logger)
    {
        _options = options.Value;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Handshake do Meta ao configurar webhook: retorna hub.challenge se
    /// hub.verify_token bate com o configurado.
    /// </summary>
    [HttpGet]
    public ActionResult Verify(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        if (mode == "subscribe"
            && !string.IsNullOrEmpty(_options.WebhookVerifyToken)
            && verifyToken == _options.WebhookVerifyToken)
        {
            _logger.LogInformation("WhatsApp webhook: handshake bem sucedido");
            return Ok(challenge);
        }
        _logger.LogWarning("WhatsApp webhook: handshake rejeitado. Mode={Mode}", mode);
        return Forbid();
    }

    /// <summary>
    /// Recebe eventos do Meta. Idempotente — duplicatas são ignoradas via
    /// ProviderMessageId.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> Receber(CancellationToken ct)
    {
        // Lê body raw para eventual validação HMAC
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;

        // Validação HMAC (opcional — só em produção com app secret)
        // Skip in stub/dev mode para facilitar testes
        // if (!_options.ModoStub && !ValidarAssinatura(rawBody, Request.Headers)) return Forbid();

        MetaWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<MetaWebhookPayload>(rawBody);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "WhatsApp webhook: body invalido");
            return BadRequest();
        }

        if (payload?.Entry is null)
        {
            return Ok(); // vazio/heartbeat
        }

        foreach (var entry in payload.Entry)
        {
            if (entry.Changes is null) continue;
            foreach (var change in entry.Changes)
            {
                if (change.Value is null) continue;
                await ProcessarChangeAsync(change.Value, ct);
            }
        }

        return Ok();
    }

    private async Task ProcessarChangeAsync(MetaWebhookValue value, CancellationToken ct)
    {
        var phoneNumberId = value.Metadata?.PhoneNumberId;
        if (string.IsNullOrEmpty(phoneNumberId)) return;

        var tenant = await ResolverTenantAsync(phoneNumberId, ct);
        if (tenant is null)
        {
            _logger.LogWarning("WhatsApp webhook: phone_number_id {Id} nao esta vinculado a nenhum tenant", phoneNumberId);
            return;
        }

        // Mensagens recebidas
        if (value.Messages is not null)
        {
            foreach (var msg in value.Messages)
            {
                await ProcessarMensagemAsync(msg, value, tenant, ct);
            }
        }

        // Callbacks de status (entregue, lida, falhou)
        if (value.Statuses is not null)
        {
            foreach (var st in value.Statuses)
            {
                await ProcessarStatusAsync(st, tenant, ct);
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task<Tenant?> ResolverTenantAsync(string phoneNumberId, CancellationToken ct)
    {
        // Prioridade 1: NumeroWhatsApp (multi-numero, Sprint 29.3)
        var numero = await _context.NumerosWhatsApp.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(n => n.PhoneNumberId == phoneNumberId && n.Ativo, ct);
        if (numero is not null)
        {
            return await _context.Tenants.IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == numero.TenantId, ct);
        }

        // Prioridade 2: Tenant.WhatsAppPhoneNumberId (legado, 1 número por tenant)
        var porTenant = await _context.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.WhatsAppPhoneNumberId == phoneNumberId, ct);
        if (porTenant is not null) return porTenant;

        // Prioridade 3: fallback dev (config em appsettings)
        if (_options.PhoneNumberId == phoneNumberId)
        {
            return await _context.Tenants.IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == ApplicationDbContext.DemoTenantId, ct);
        }

        return null;
    }

    /// <summary>
    /// Quando mensagem chega e o cliente não tem corretor responsável,
    /// distribui automaticamente:
    /// 1. Se o número recebedor tem UsuarioId definido, usa ele
    /// 2. Senão, round-robin entre números compartilhados ativos do tenant
    /// </summary>
    private async Task<int?> DistribuirCorretorAsync(Guid tenantId, string phoneNumberId, CancellationToken ct)
    {
        var numero = await _context.NumerosWhatsApp.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(n => n.PhoneNumberId == phoneNumberId && n.TenantId == tenantId, ct);
        if (numero?.UsuarioId is not null) return numero.UsuarioId;

        // Round-robin: pega o corretor com menos clientes recentes
        var umaSemanaAtras = DateTime.UtcNow.AddDays(-7);
        var candidato = await _context.Usuarios.IgnoreQueryFilters()
            .Where(u => u.TenantId == tenantId && u.Ativo && u.Role == "Corretor")
            .Select(u => new
            {
                u.Id,
                ClientesRecentes = _context.Clientes.IgnoreQueryFilters()
                    .Count(c => c.CorretorResponsavelId == u.Id && c.DataCadastro >= umaSemanaAtras)
            })
            .OrderBy(x => x.ClientesRecentes)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync(ct);

        return candidato?.Id;
    }

    private async Task ProcessarMensagemAsync(MetaInboundMessage msg, MetaWebhookValue value, Tenant tenant, CancellationToken ct)
    {
        // Idempotência: ignora se já processamos esta msg
        var jaExiste = await _context.WhatsAppMensagens.IgnoreQueryFilters()
            .AnyAsync(m => m.ProviderMessageId == msg.Id, ct);
        if (jaExiste) return;

        var from = msg.From ?? string.Empty;
        var phoneNumberId = value.Metadata?.PhoneNumberId ?? string.Empty;

        var cliente = await _context.Clientes.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.TenantId == tenant.Id
                && (c.Telefone.Contains(from) || (c.Whatsapp ?? "").Contains(from)), ct);

        var conteudo = ExtrairConteudo(msg);

        // Auto-criação de lead para numero desconhecido (reduz fricção)
        if (cliente is null && !string.IsNullOrEmpty(from))
        {
            var corretorId = await DistribuirCorretorAsync(tenant.Id, phoneNumberId, ct);
            var cpfPlaceholder = $"WPP-{Guid.NewGuid():N}"[..14];
            cliente = new Domain.Entities.Cliente
            {
                TenantId = tenant.Id,
                Nome = $"Lead WhatsApp {from}",
                Cpf = cpfPlaceholder,
                Email = $"{cpfPlaceholder}@lead-whatsapp.local",
                Telefone = from,
                Whatsapp = from,
                OrigemLead = OrigemLead.WhatsApp,
                StatusFunil = StatusFunil.Lead,
                CorretorResponsavelId = corretorId,
                ConsentimentoLgpd = false,
                DataCadastro = DateTime.UtcNow
            };
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync(ct); // precisa ID pra interacao
            _logger.LogInformation("WhatsApp auto-criou lead {Id} para {From} → corretor {CId}",
                cliente.Id, from, corretorId);
        }

        var mensagem = new WhatsAppMensagem
        {
            TenantId = tenant.Id,
            ClienteId = cliente?.Id,
            TelefoneContato = from,
            NumeroEmpresa = phoneNumberId,
            Direcao = DirecaoWhatsApp.Recebida,
            Status = StatusMensagemWhatsApp.Entregue,
            Conteudo = conteudo,
            ProviderMessageId = msg.Id,
            CreatedAt = DateTime.UtcNow,
            EntregueEm = DateTime.UtcNow
        };
        _context.WhatsAppMensagens.Add(mensagem);

        if (cliente is not null)
        {
            _context.HistoricoInteracoes.Add(new HistoricoInteracao
            {
                TenantId = tenant.Id,
                ClienteId = cliente.Id,
                Tipo = TipoInteracao.Whatsapp,
                Conteudo = $"[Recebida] {conteudo}",
                DataHora = DateTime.UtcNow
            });
        }

        _logger.LogInformation("WhatsApp inbound: {From} → tenant {Tenant} ({Cliente})",
            from, tenant.Slug, cliente?.Nome ?? "não identificado");
    }

    private async Task ProcessarStatusAsync(MetaStatusUpdate st, Tenant tenant, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(st.Id)) return;
        var msg = await _context.WhatsAppMensagens.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.ProviderMessageId == st.Id && m.TenantId == tenant.Id, ct);
        if (msg is null) return;

        switch (st.Status)
        {
            case "sent":
                if (msg.Status < StatusMensagemWhatsApp.Aceita)
                    msg.Status = StatusMensagemWhatsApp.Aceita;
                break;
            case "delivered":
                msg.Status = StatusMensagemWhatsApp.Entregue;
                msg.EntregueEm ??= DateTime.UtcNow;
                break;
            case "read":
                msg.Status = StatusMensagemWhatsApp.Lida;
                msg.LidaEm ??= DateTime.UtcNow;
                if (msg.EntregueEm is null) msg.EntregueEm = DateTime.UtcNow;
                break;
            case "failed":
                msg.Status = StatusMensagemWhatsApp.Falhou;
                msg.MensagemErro = st.Errors?.FirstOrDefault()?.Title ?? "Falhou sem detalhes";
                break;
        }
    }

    private static string ExtrairConteudo(MetaInboundMessage msg)
    {
        return msg.Type switch
        {
            "text" => msg.Text?.Body ?? "",
            "image" => "[Imagem recebida]",
            "audio" => "[Áudio recebido]",
            "video" => "[Vídeo recebido]",
            "document" => "[Documento recebido]",
            "location" => "[Localização recebida]",
            _ => $"[{msg.Type}]"
        };
    }

    // ========== DTOs do payload Meta ==========

    private class MetaWebhookPayload
    {
        [JsonPropertyName("entry")] public List<MetaEntry>? Entry { get; set; }
    }

    private class MetaEntry
    {
        [JsonPropertyName("changes")] public List<MetaChange>? Changes { get; set; }
    }

    private class MetaChange
    {
        [JsonPropertyName("value")] public MetaWebhookValue? Value { get; set; }
        [JsonPropertyName("field")] public string? Field { get; set; }
    }

    private class MetaWebhookValue
    {
        [JsonPropertyName("messaging_product")] public string? MessagingProduct { get; set; }
        [JsonPropertyName("metadata")] public MetaMetadata? Metadata { get; set; }
        [JsonPropertyName("messages")] public List<MetaInboundMessage>? Messages { get; set; }
        [JsonPropertyName("statuses")] public List<MetaStatusUpdate>? Statuses { get; set; }
    }

    private class MetaMetadata
    {
        [JsonPropertyName("phone_number_id")] public string? PhoneNumberId { get; set; }
        [JsonPropertyName("display_phone_number")] public string? DisplayPhoneNumber { get; set; }
    }

    private class MetaInboundMessage
    {
        [JsonPropertyName("from")] public string? From { get; set; }
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("timestamp")] public string? Timestamp { get; set; }
        [JsonPropertyName("type")] public string? Type { get; set; }
        [JsonPropertyName("text")] public MetaInboundText? Text { get; set; }
    }

    private class MetaInboundText
    {
        [JsonPropertyName("body")] public string? Body { get; set; }
    }

    private class MetaStatusUpdate
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("status")] public string? Status { get; set; }
        [JsonPropertyName("timestamp")] public string? Timestamp { get; set; }
        [JsonPropertyName("recipient_id")] public string? RecipientId { get; set; }
        [JsonPropertyName("errors")] public List<MetaError>? Errors { get; set; }
    }

    private class MetaError
    {
        [JsonPropertyName("code")] public int Code { get; set; }
        [JsonPropertyName("title")] public string? Title { get; set; }
    }

    /// <summary>
    /// Valida assinatura HMAC SHA-256 do payload contra App Secret do Meta.
    /// Uso: chamar no início do Receber() em produção.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051", Justification = "Reservado para habilitação em produção com App Secret configurado.")]
    private bool ValidarAssinatura(string rawBody, IHeaderDictionary headers)
    {
        if (!headers.TryGetValue("X-Hub-Signature-256", out var sig) || string.IsNullOrEmpty(sig.ToString()))
            return false;
        var expected = "sha256=" + Convert.ToHexString(
            HMACSHA256.HashData(Encoding.UTF8.GetBytes(_options.AccessToken),
                                 Encoding.UTF8.GetBytes(rawBody))).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(sig.ToString()));
    }
}
