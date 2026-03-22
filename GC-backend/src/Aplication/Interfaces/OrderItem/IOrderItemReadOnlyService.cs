using Applications.dtos;

namespace Aplication.Interfaces.OrderItem;

public interface IOrderItemReadOnlyService
{
    Task<IEnumerable<OrderItemDTO>> GetByOrderIdAsync(int orderId);
}
