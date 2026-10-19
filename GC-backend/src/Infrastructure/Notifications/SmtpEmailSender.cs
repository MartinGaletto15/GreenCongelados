using Application.Interfaces.EmailSender;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Infrastructure.Notifications;

public class SmtpEmailSender : IEmailSenderService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendPasswordResetEmailAsync(string to, string token)
    {
        string email = _configuration["MailKit:Email"]
            ?? throw new InvalidOperationException("No se encontró el Email para enviar el correo.");

        string? passwordGmail = _configuration["MailKit:PasswordGmail"]
            ?? Environment.GetEnvironmentVariable("MailKit__PasswordGmail");
        if (string.IsNullOrWhiteSpace(passwordGmail))
        {
            throw new InvalidOperationException("No se encontró la contraseña de Gmail. Configúrala en User Secrets o en la variable de entorno MailKit__PasswordGmail.");
        }

        string server = _configuration["MailKit:Server"]
            ?? Environment.GetEnvironmentVariable("MailKit__Server")
            ?? throw new InvalidOperationException("No se encontró el servidor SMTP.");
        int serverPort = int.TryParse(_configuration["MailKit:Port"] ?? Environment.GetEnvironmentVariable("MailKit__Port"), out int parsedPort)
            ? parsedPort
            : throw new InvalidOperationException("No se encontró el puerto SMTP.");

        // Construir el mensaje
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("GreenCongelados", email));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = "Restablecimiento de contraseña - GreenCongelados";

        message.Body = new TextPart("html")
        {
            Text = $@"
            <p>Hola,</p>
            <p>Has solicitado restablecer tu contraseña. Aquí está tu token de recuperación:</p>
            <p style=""font-family: monospace; font-size: 16px; background-color: #f4f4f4; padding: 10px; border-radius: 4px;"">
            <b>{token}</b>
            </p>
            <p>Si no solicitaste este cambio, puedes ignorar este correo.</p>"
        };

        // Enviar correo vía SMTP
        using var client = new SmtpClient();
        await client.ConnectAsync(server, serverPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(email, passwordGmail);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}