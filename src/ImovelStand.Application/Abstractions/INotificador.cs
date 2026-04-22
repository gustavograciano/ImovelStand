namespace ImovelStand.Application.Abstractions;

public interface INotificador
{
    Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml, CancellationToken cancellationToken = default);
    Task EnviarWhatsappAsync(string telefone, string mensagem, CancellationToken cancellationToken = default);
}

public class NotificacaoOptions
{
    public SmtpOptions Smtp { get; set; } = new();
    public WhatsappOptions Whatsapp { get; set; } = new();
}

public class SmtpOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public string? Usuario { get; set; }
    public string? Senha { get; set; }
    public bool UseSsl { get; set; }
    public string From { get; set; } = "no-reply@imovelstand.com";
    public string FromNome { get; set; } = "ImovelStand";
}

public class WhatsappOptions
{
    public string? Provider { get; set; }   // "z-api", "twilio" futuro
    public string? ApiUrl { get; set; }
    public string? Token { get; set; }
    public string? InstanceId { get; set; }
}
