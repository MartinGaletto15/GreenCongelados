using Applications.dtos;
using Domain.Enums.Entities;

namespace Aplication.Interfaces.User;

public interface ICadetWriteService
{
    Task<UserDTO> AsingOrderToCadetAsync(int id, int idOrder);
    Task<OrderDTO> UpdateOrderStatusAsync(int idCadet, int idOrder, OrderStatus status);
}
