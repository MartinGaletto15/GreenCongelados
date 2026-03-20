using System.Linq;
using System.Text.RegularExpressions;

namespace Aplication.Validators;

public static partial class Validations
{
    public static bool ValidateString(string? str, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(str))
            return false;

        if (minLength > 0 && str.Length < minLength)
            return false;
            
        if (maxLength > 0 && str.Length > maxLength)
            return false;
            
        return true;
    }

    public static bool ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex().IsMatch(email);
    }

    public static bool ValidatePassword(string? password, int minLength, int maxLength, bool needsUppercase, bool needsNumber)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        if (minLength > 0 && password.Length < minLength)
            return false;
            
        if (maxLength > 0 && password.Length > maxLength)
            return false;
            
        if (needsUppercase && !password.Any(char.IsUpper))
            return false;
            
        if (needsNumber && !password.Any(char.IsDigit))
            return false;

        return true;
    }

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailRegex();
}
