namespace Application.Interfaces.Security;

public interface IRevokedUserStore
{
    void Revoke(int userId);

    bool IsRevoked(int userId);
}
