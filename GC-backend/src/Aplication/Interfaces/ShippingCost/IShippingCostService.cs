using Applications.dtos;
using Applications.dtos.Requests;

namespace Aplication.Interfaces.ShippingCost;

public interface IShippingCostService
{
    Task<IEnumerable<ShippingCostDTO>> GetAllAsync();
    Task<ShippingCostDTO> GetByIdAsync(int id);
    Task<ShippingCostDTO> CreateAsync(CreateShippingCostRequest request);
    Task<ShippingCostDTO> UpdateAsync(int id, UpdateShippingCostRequest request);
    Task DeleteAsync(int id);
}
