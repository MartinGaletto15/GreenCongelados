using Domain.Entities;

namespace Domain.Interfaces;

public interface IPasswordResetTokenRepository: IGenericRepository<PasswordResetToken>
{
    Task<PasswordResetToken?> GetByTokenAsync(int token);
    Task<IEnumerable<PasswordResetToken>>GetActiveByUserIdAsync(int userId);
    Task InvalidateAllForUserAsync(int userId);
    Task DeleteExpiredAsync();
}