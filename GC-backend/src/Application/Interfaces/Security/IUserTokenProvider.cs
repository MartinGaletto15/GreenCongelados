namespace Application.Interfaces.Security;

public interface IUserTokenProvider
{
    string GenerateToken();
    string HashToken(string token);
}
