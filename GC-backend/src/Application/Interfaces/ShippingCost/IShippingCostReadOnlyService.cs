using Application.DTOs;

namespace Application.Interfaces.ShippingCost;

public interface IShippingCostReadOnlyService
{
    Task<IEnumerable<ShippingCostDTO>> GetAllAsync();
    Task<ShippingCostDTO> GetByIdAsync(int id);
    Task<ShippingCostDTO?> GetActiveAsync();
}