using Application.Interfaces.EmailSender;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Notifications;

public class SmtpEmailSender : IEmailSenderService
{
    private readonly SmtpOptions _options;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendPasswordResetEmailAsync(string to, string token)
    {
        string subject = "Restablecimiento de contraseña - GreenCongelados";
        string htmlBody = $@"
            <p>Hola,</p>
            <p>Has solicitado restablecer tu contraseña. Aquí está tu token de recuperación:</p>
            <p style=""font-family: monospace; font-size: 16px; background-color: #f4f4f4; padding: 10px; border-radius: 4px;"">
                <b>{token}</b>
            </p>
            <p>Si no solicitaste este cambio, puedes ignorar este correo.</p>";

        await SendEmailAsync(to, subject, htmlBody);
    }

    public async Task SendEmailConfirmationAsync(string to, string token)
    {
        string subject = "Confirma tu cuenta - GreenCongelados";
        string htmlBody = $@"
            <p>Hola,</p>
            <p>Gracias por registrarte en GreenCongelados. Usa el siguiente token para confirmar tu dirección de correo electrónico:</p>
            <p style=""font-family: monospace; font-size: 16px; background-color: #f4f4f4; padding: 10px; border-radius: 4px;"">
                <b>{token}</b>
            </p>
            <p>Si no creaste una cuenta, puedes ignorar este correo.</p>";

        await SendEmailAsync(to, subject, htmlBody);
    }

    private async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_options.Email))
            throw new InvalidOperationException("No se encontró MailKit:Email para enviar el correo.");

        if (string.IsNullOrWhiteSpace(_options.PasswordGmail))
            throw new InvalidOperationException("No se encontró MailKit:PasswordGmail. Configúralo en User Secrets o en la variable de entorno MailKit__PasswordGmail.");

        if (string.IsNullOrWhiteSpace(_options.Server))
            throw new InvalidOperationException("No se encontró MailKit:Server.");

        if (_options.Port <= 0)
            throw new InvalidOperationException("MailKit:Port debe configurarse con un valor mayor que cero.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("GreenCongelados", _options.Email));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = htmlBody
        };

        using var client = new SmtpClient
        {
            Timeout = Math.Max(1, _options.TimeoutSeconds) * 1000
        };

        // El envío es best-effort y no admite cancelación: una vez confirmado el
        // usuario y persistido el token, se intenta completar la entrega. La
        // duración máxima la acota client.Timeout (MailKit:TimeoutSeconds).
        try
        {
            await client.ConnectAsync(_options.Server, _options.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_options.Email, _options.PasswordGmail);
            await client.SendAsync(message);
        }
        finally
        {
            if (client.IsConnected)
            {
                try
                {
                    await client.DisconnectAsync(true);
                }
                catch
                {
                    // No ocultar la excepción original del envío si falla el cierre de sesión.
                }
            }
        }
    }
}
