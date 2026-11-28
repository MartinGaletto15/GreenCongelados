using Application.DTOs;
using Application.DTOs.Requests;

namespace Application.Interfaces.OrderItem;

public interface IOrderItemWriteService
{
    Task<OrderItemDTO> CreateAsync(CreateOrderItemRequest request);
}
