using Domain.Entities;

namespace Domain.Interfaces;

public interface IPromotionRepository : IGenericRepository<Promotion>
{
    Task<Promotion?> GetByCodeAsync(string couponCode);
    Task<bool> ExistsByCodeAsync(string couponCode, int? excludedPromotionId = null);
}
