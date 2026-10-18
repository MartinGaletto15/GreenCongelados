using Application.DTOs;
using Domain.Entities.Enums;

namespace Application.Interfaces.User;

public interface ICadetWriteService
{
    Task<UserDTO> AsingOrderToCadetAsync(int id, int idOrder);
    Task<OrderDTO> UpdateOrderStatusAsync(int idCadet, int idOrder, OrderStatus status);
}
