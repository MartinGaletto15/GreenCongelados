using Application.Interfaces.EmailSender;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net;

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
        string safeToken = WebUtility.HtmlEncode(token);
        string htmlBody = $@"
            <!DOCTYPE html>
            <html lang=""es"">
            <body style=""margin: 0; padding: 32px 16px; background-color: #f3f7f4; font-family: Arial, Helvetica, sans-serif; color: #26352b;"">
                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width: 560px; margin: 0 auto; background-color: #ffffff; border-radius: 12px;"">
                    <tr>
                        <td style=""padding: 32px 36px;"">
                            <p style=""margin: 0 0 8px; color: #287a45; font-size: 14px; font-weight: bold; letter-spacing: 1px;"">GREENCONGELADOS</p>
                            <h1 style=""margin: 0 0 20px; font-size: 24px; color: #203b2a;"">Restablece tu contraseña</h1>
                            <p style=""margin: 0 0 16px; font-size: 16px; line-height: 1.6;"">Hola,</p>
                            <p style=""margin: 0 0 24px; font-size: 16px; line-height: 1.6;"">
                                Has solicitado restablecer tu contraseña. Usa este código para continuar:
                            </p>
                            <div style=""margin: 0 0 24px; padding: 18px; border: 1px solid #dce9df; border-radius: 8px; background-color: #f5faf6; text-align: center;"">
                                <span style=""font-family: Consolas, Monaco, monospace; font-size: 24px; font-weight: bold; letter-spacing: 3px; color: #287a45;"">{safeToken}</span>
                            </div>
                            <hr style=""border: 0; border-top: 1px solid #e8eee9; margin: 0 0 20px;"" />
                            <p style=""margin: 0; font-size: 13px; line-height: 1.6; color: #718076;"">
                                ¿No solicitaste este cambio? Puedes ignorar este mensaje; tu contraseña no cambiará.
                            </p>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";

        await SendEmailAsync(to, subject, htmlBody);
    }

    public async Task SendEmailConfirmationAsync(string to, string token)
    {
        string subject = "Confirma tu cuenta - GreenCongelados";
        string safeToken = WebUtility.HtmlEncode(token);
        string htmlBody = $@"
            <!DOCTYPE html>
            <html lang=""es"">
            <body style=""margin: 0; padding: 32px 16px; background-color: #f3f7f4; font-family: Arial, Helvetica, sans-serif; color: #26352b;"">
                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width: 560px; margin: 0 auto; background-color: #ffffff; border-radius: 12px;"">
                    <tr>
                        <td style=""padding: 32px 36px;"">
                            <p style=""margin: 0 0 8px; color: #287a45; font-size: 14px; font-weight: bold; letter-spacing: 1px;"">GREENCONGELADOS</p>
                            <h1 style=""margin: 0 0 20px; font-size: 24px; color: #203b2a;"">¡Un paso más para empezar!</h1>
                            <p style=""margin: 0 0 16px; font-size: 16px; line-height: 1.6;"">Hola,</p>
                            <p style=""margin: 0 0 24px; font-size: 16px; line-height: 1.6;"">
                                Gracias por sumarte a GreenCongelados. Confirma tu correo electrónico con este código:
                            </p>
                            <div style=""margin: 0 0 24px; padding: 18px; border: 1px solid #dce9df; border-radius: 8px; background-color: #f5faf6; text-align: center;"">
                                <span style=""font-family: Consolas, Monaco, monospace; font-size: 24px; font-weight: bold; letter-spacing: 3px; color: #287a45;"">{safeToken}</span>
                            </div>
                            <p style=""margin: 0 0 24px; font-size: 14px; line-height: 1.6; color: #5b6b60;"">
                                Copia el código y pégalo en la pantalla de confirmación para activar tu cuenta.
                            </p>
                            <hr style=""border: 0; border-top: 1px solid #e8eee9; margin: 0 0 20px;"" />
                            <p style=""margin: 0; font-size: 13px; line-height: 1.6; color: #718076;"">
                                ¿No creaste una cuenta? Puedes ignorar este mensaje; no se realizará ningún cambio.
                            </p>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";

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
