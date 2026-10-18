using Application.DTOs;
using Application.DTOs.Requests;

namespace Application.Interfaces.ShippingCost;

public interface IShippingCostWriteService
{
    Task<ShippingCostDTO> CreateAsync(CreateShippingCostRequest request);
    Task<ShippingCostDTO> UpdateAsync(int id, UpdateShippingCostRequest request);
    Task DeleteAsync(int id);
}
