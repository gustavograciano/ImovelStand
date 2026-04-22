using System.Net.Http.Json;
using ImovelStand.Application.Abstractions;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ImovelStand.Infrastructure.Notificacoes;

/// <summary>
/// Implementação de <see cref="INotificador"/> usando MailKit (SMTP) para e-mail
/// e Z-API (HTTP) para WhatsApp. Em ambiente sem credenciais configuradas,
/// apenas loga e ignora — útil para dev sem depender de infra externa.
/// </summary>
public class MailKitNotificador : INotificador
{
    private readonly NotificacaoOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MailKitNotificador> _logger;

    public MailKitNotificador(
        IOptions<NotificacaoOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<MailKitNotificador> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Smtp.Host))
        {
            _logger.LogInformation("SMTP não configurado. Email {Assunto} para {Dest} ignorado.", assunto, destinatario);
            return;
        }

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_options.Smtp.FromNome, _options.Smtp.From));
        msg.To.Add(MailboxAddress.Parse(destinatario));
        msg.Subject = assunto;
        msg.Body = new BodyBuilder { HtmlBody = corpoHtml }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.Smtp.Host, _options.Smtp.Port, _options.Smtp.UseSsl, cancellationToken);
        if (!string.IsNullOrEmpty(_options.Smtp.Usuario))
            await client.AuthenticateAsync(_options.Smtp.Usuario, _options.Smtp.Senha, cancellationToken);
        await client.SendAsync(msg, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
        _logger.LogInformation("Email enviado: {Dest} {Assunto}", destinatario, assunto);
    }

    public async Task EnviarWhatsappAsync(string telefone, string mensagem, CancellationToken cancellationToken = default)
    {
        var w = _options.Whatsapp;
        if (string.IsNullOrWhiteSpace(w.ApiUrl) || string.IsNullOrWhiteSpace(w.Token))
        {
            _logger.LogInformation("WhatsApp não configurado. Mensagem para {Tel} ignorada.", telefone);
            return;
        }

        var url = w.Provider switch
        {
            "z-api" => $"{w.ApiUrl.TrimEnd('/')}/instances/{w.InstanceId}/token/{w.Token}/send-text",
            _ => w.ApiUrl
        };

        var http = _httpClientFactory.CreateClient();
        var response = await http.PostAsJsonAsync(url, new { phone = telefone, message = mensagem }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Falha ao enviar WhatsApp para {Tel}: {Status}", telefone, response.StatusCode);
            return;
        }
        _logger.LogInformation("WhatsApp enviado: {Tel}", telefone);
    }
}
