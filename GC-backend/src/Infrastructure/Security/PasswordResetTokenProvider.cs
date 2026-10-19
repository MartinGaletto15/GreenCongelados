using Application.Interfaces.PasswordReset;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Security;

public class PasswordResetTokenProvider : IPasswordResetTokenProvider
{
    public string GenerateToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(randomBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}
