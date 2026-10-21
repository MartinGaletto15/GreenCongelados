using Application.Interfaces.EmailSender;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

/// <summary>
/// Envuelve al sender real para que un fallo de SMTP no haga fallar la petición.
/// El usuario y el token ya están persistidos cuando se envía el correo, y el
/// usuario puede reintentar mediante el endpoint de reenvío.
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

    public Task SendPasswordResetEmailAsync(string to, string token, CancellationToken cancellationToken = default)
    {
        return TrySendAsync(
            () => _inner.SendPasswordResetEmailAsync(to, token, cancellationToken),
            to,
            "restablecimiento de contraseña");
    }

    public Task SendEmailConfirmationAsync(string to, string token, CancellationToken cancellationToken = default)
    {
        return TrySendAsync(
            () => _inner.SendEmailConfirmationAsync(to, token, cancellationToken),
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
