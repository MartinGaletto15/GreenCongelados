namespace Application.Interfaces.EmailSender;

public interface IEmailSenderService
{
    Task SendPasswordResetEmailAsync(string to, string token, CancellationToken cancellationToken = default);
    Task SendEmailConfirmationAsync(string to, string token, CancellationToken cancellationToken = default);
}