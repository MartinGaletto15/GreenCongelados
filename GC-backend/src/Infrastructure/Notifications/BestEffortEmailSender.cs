using Application.Interfaces.EmailSender;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

/// <summary>
/// Envuelve al sender real para que un fallo de SMTP no haga fallar la petición.
/// El usuario y el token ya están persistidos cuando se envía el correo, y el
/// usuario puede reintentar mediante el endpoint de reenvío. Por eso el envío
/// se intenta completar siempre: no se cancela aunque el cliente abandone la
/// petición, y cualquier error se registra en el log sin propagarse.
/// </summary>
public sealed class BestEffortEmailSender : IEmailSenderService
{
    private readonly SmtpEmailSender _inner;
    private readonly ILogger<BestEffortEmailSender> _logger;

    public BestEffortEmailSender(SmtpEmailSender inner, ILogger<BestEffortEmailSender> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string to, string token)
    {
        return TrySendAsync(
            () => _inner.SendPasswordResetEmailAsync(to, token),
            to,
            "restablecimiento de contraseña");
    }

    public Task SendEmailConfirmationAsync(string to, string token)
    {
        return TrySendAsync(
            () => _inner.SendEmailConfirmationAsync(to, token),
            to,
            "confirmación de cuenta");
    }

    private async Task TrySendAsync(Func<Task> send, string to, string kind)
    {
        try
        {
            await send();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar el correo de {Kind} a {Email}. El usuario puede solicitar un reenvío.", kind, to);
        }
    }
}
