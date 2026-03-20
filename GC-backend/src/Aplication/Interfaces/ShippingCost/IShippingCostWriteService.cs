using Applications.dtos;
using Applications.dtos.Requests;

namespace Aplication.Interfaces.ShippingCost;

public interface IShippingCostWriteService
{
    Task<ShippingCostDTO> CreateAsync(CreateShippingCostRequest request);
    Task<ShippingCostDTO> UpdateAsync(int id, UpdateShippingCostRequest request);
    Task DeleteAsync(int id);
}
