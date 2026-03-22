using Applications.dtos;
using Applications.dtos.Requests;

namespace Aplication.Interfaces.OrderItem;

public interface IOrderItemWriteService
{
    Task<OrderItemDTO> CreateAsync(CreateOrderItemRequest request);
}
