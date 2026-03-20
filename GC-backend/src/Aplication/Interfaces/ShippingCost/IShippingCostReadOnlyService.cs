using Applications.dtos;

namespace Aplication.Interfaces.ShippingCost;

public interface IShippingCostReadOnlyService
{
    Task<IEnumerable<ShippingCostDTO>> GetAllAsync();
    Task<ShippingCostDTO> GetByIdAsync(int id);
}