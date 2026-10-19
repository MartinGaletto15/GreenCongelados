using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PasswordResetTokenRepository : GenericRepository<PasswordResetToken>, IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(GCContext context) : base(context)
    {
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task<IEnumerable<PasswordResetToken>> GetActiveByUserIdAsync(int userId)
    {
        return await _context.PasswordResetTokens
            .Where(x => x.IdUser == userId && !x.IsUsed && x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task InvalidateAllForUserAsync(int userId)
    {
        var activeTokens = await _context.PasswordResetTokens
            .Where(x => x.IdUser == userId && !x.IsUsed)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.MarkAsUsed();
        }
    }

    public async Task DeleteExpiredAsync()
    {
        var expiredTokens = await _context.PasswordResetTokens
            .Where(x => x.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        if (expiredTokens.Count == 0)
        {
            return;
        }

        _context.PasswordResetTokens.RemoveRange(expiredTokens);
    }
}