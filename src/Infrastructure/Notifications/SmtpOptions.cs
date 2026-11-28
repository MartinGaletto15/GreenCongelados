namespace Infrastructure.Notifications;

public class SmtpOptions
{
    public const string SectionName = "MailKit";

    public string Email { get; set; } = string.Empty;
    public string PasswordGmail { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public int TimeoutSeconds { get; set; } = 15;
}
