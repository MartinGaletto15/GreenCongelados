using Applications.dtos;
using Applications.dtos.Requests;

namespace Aplication.Interfaces.OrderItem;

public interface IOrderItemService
{
    Task<IEnumerable<OrderItemDTO>> GetAllAsync();
    Task<OrderItemDTO> GetByIdAsync(int id);
    Task<OrderItemDTO> CreateAsync(CreateOrderItemRequest request);
    Task<OrderItemDTO> UpdateAsync(int id, UpdateOrderItemRequest request);
    Task DeleteAsync(int id);
}
