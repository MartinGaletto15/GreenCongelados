using Applications.dtos;
using Applications.dtos.Requests;

namespace Aplication.Interfaces.Order;

public interface IOrderService
{
    Task<IEnumerable<OrderDTO>> GetAllAsync();
    Task<OrderDTO> GetByIdAsync(int id);
    Task<OrderDTO> CreateAsync(CreateOrderRequest request);
    Task<OrderDTO> UpdateAsync(int id, UpdateOrderRequest request);
    Task DeleteAsync(int id);
}
