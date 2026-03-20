using Applications.dtos;
using Applications.dtos.Requests;

namespace Aplication.Interfaces.Promotion;

public interface IPromotionWriteService
{
    Task<PromotionDTO> CreateAsync(CreatePromotionRequest request);
    Task<PromotionDTO> UpdateAsync(int id, UpdatePromotionRequest request);
    Task DeleteAsync(int id);
}