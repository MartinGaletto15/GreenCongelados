using Application.DTOs;

namespace Application.Interfaces.Promotion;

public interface IPromotionReadOnlyService
{
    Task<IEnumerable<PromotionDTO>> GetAllAsync();
    Task<PromotionDTO> GetByIdAsync(int id);
    Task<PromotionDTO?> GetByCodeAsync(string couponCode);
}