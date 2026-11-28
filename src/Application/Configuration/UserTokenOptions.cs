namespace Application.Configuration;

public class UserTokenOptions
{
    public const string SectionName = "UserTokens";

    public int PasswordResetExpirationMinutes { get; set; }
    public int EmailConfirmationExpirationMinutes { get; set; }
}
