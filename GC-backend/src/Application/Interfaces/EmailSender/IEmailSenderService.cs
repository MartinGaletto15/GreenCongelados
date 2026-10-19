namespace Application.Interfaces.EmailSender;

public interface IEmailSenderService
{
    Task SendPasswordResetEmailAsync(string to, string tokenUrl);
}