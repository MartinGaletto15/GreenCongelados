using Domain.Entities;

namespace Application.Interfaces.Security;

public interface IJwtProvider
{
    string GenerateToken(Domain.Entities.User user);
}
