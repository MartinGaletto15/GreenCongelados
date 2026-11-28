using Domain.Entities;
using Domain.Entities.Enums;

namespace Domain.Interfaces;

public interface IUserTokenRepository: IGenericRepository<UserToken>
{
    Task<UserToken?> GetByTokenAsync(string token, TokenType tokenType);
    Task<IEnumerable<UserToken>> GetActiveByUserIdAsync(int userId, TokenType tokenType);
    Task InvalidateAllForUserAsync(int userId, TokenType tokenType);
    Task DeleteExpiredAsync(TokenType tokenType);
}