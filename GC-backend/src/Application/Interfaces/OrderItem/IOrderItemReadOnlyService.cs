using Application.DTOs;

namespace Application.Interfaces.OrderItem;

public interface IOrderItemReadOnlyService
{
    Task<IEnumerable<OrderItemDTO>> GetByOrderIdAsync(int orderId);
}
