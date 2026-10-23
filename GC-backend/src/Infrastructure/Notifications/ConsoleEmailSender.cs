using Application.Interfaces.EmailSender;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

/// <summary>
/// Sender de desarrollo: no envía correos reales, solo registra el token en el log.
/// Se usa cuando MailKit:PasswordGmail no está configurado en Development.
/// </summary>
public sealed class ConsoleEmailSender : IEmailSenderService
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string to, string token)
    {
        _logger.LogWarning("[DEV] Correo de restablecimiento para {Email}. Token: {Token}", to, token);
        return Task.CompletedTask;
    }

    public Task SendEmailConfirmationAsync(string to, string token)
    {
        _logger.LogWarning("[DEV] Correo de confirmación para {Email}. Token: {Token}", to, token);
        return Task.CompletedTask;
    }
}
