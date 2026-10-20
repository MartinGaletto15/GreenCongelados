using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserTokenRepository : GenericRepository<UserToken>, IUserTokenRepository
{
    public UserTokenRepository(GCContext context) : base(context)
    {
    }

    public async Task<UserToken?> GetByTokenAsync(string token, TokenType typeToken)
    {
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(x => x.Token == token && x.Type == typeToken);
    }

    public async Task<IEnumerable<UserToken>> GetActiveByUserIdAsync(int userId, TokenType typeToken)
    {
        return await _context.PasswordResetTokens
            .Where(x => x.IdUser == userId && x.Type == typeToken && !x.IsUsed && x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task InvalidateAllForUserAsync(int userId, TokenType typeToken)
    {
        var activeTokens = await _context.PasswordResetTokens
            .Where(x => x.IdUser == userId && x.Type == typeToken && !x.IsUsed)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.MarkAsUsed();
        }
    }

    public async Task DeleteExpiredAsync(TokenType typeToken)
    {
        var expiredTokens = await _context.PasswordResetTokens
            .Where(x => x.Type == typeToken && x.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        if (expiredTokens.Count == 0)
        {
            return;
        }

        _context.PasswordResetTokens.RemoveRange(expiredTokens);
    }
}