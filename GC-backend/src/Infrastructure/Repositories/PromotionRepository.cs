using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PromotionRepository : GenericRepository<Promotion>, IPromotionRepository
{
    public PromotionRepository(GCContext context) : base(context)
    {
    }

    public async Task<Promotion?> GetByCodeAsync(string couponCode)
    {
        var normalizedCode = Promotion.NormalizeCouponCode(couponCode);
        return await _context.Promotions
            .FirstOrDefaultAsync(promotion => promotion.CouponCode == normalizedCode);
    }

    public async Task<bool> ExistsByCodeAsync(string couponCode, int? excludedPromotionId = null)
    {
        var normalizedCode = Promotion.NormalizeCouponCode(couponCode);
        if (excludedPromotionId.HasValue)
        {
            return await _context.Promotions.AnyAsync(promotion =>
                promotion.IdPromotion != excludedPromotionId.Value &&
                promotion.CouponCode == normalizedCode);
        }

        return await _context.Promotions.AnyAsync(promotion =>
            promotion.CouponCode == normalizedCode);
    }
}
