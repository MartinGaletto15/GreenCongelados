namespace Application.Interfaces.EmailSender;

public interface IEmailSenderService
{
    Task SendPasswordResetEmailAsync(string to, string token);
    Task SendEmailConfirmationAsync(string to, string token);
}