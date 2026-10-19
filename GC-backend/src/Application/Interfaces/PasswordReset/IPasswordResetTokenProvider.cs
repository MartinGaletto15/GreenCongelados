namespace Application.Interfaces.PasswordReset;

public interface IPasswordResetTokenProvider
{
    string GenerateToken();
    string HashToken(string token);
}
