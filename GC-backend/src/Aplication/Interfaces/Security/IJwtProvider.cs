using Domain.Entities;

namespace Aplication.Interfaces.Security;

public interface IJwtProvider
{
    string GenerateToken(Domain.Entities.User user);
}
