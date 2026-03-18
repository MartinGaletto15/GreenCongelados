using Domain.Entities;

namespace Aplication.Interfaces.Security;

public interface IJwtProvider
{
    string GenerateToken(User user);
}
