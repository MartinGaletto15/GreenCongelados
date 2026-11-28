using Application.DTOs;
using Application.DTOs.Requests;

namespace Application.Interfaces.Promotion;

public interface IPromotionWriteService
{
    Task<PromotionDTO> CreateAsync(CreatePromotionRequest request);
    Task<PromotionDTO> UpdateAsync(int id, UpdatePromotionRequest request);
    Task DeleteAsync(int id);
}