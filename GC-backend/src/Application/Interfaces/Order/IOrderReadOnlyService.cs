using Application.DTOs;
using Domain.Entities.Enums;

namespace Application.Interfaces.Order;

public interface IOrderReadOnlyService
{
    Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
    Task<OrderDTO> GetOrderByIdAsync(int id, int userId, Role role);
    Task<IEnumerable<OrderDTO>> GetMyOrdersAsync(int userId);
    Task<OrderDTO> GetMyOrderByIdAsync(int id, int userId, Role role);
    Task<IEnumerable<OrderDTO>> GetOrdersByCourierAsync(int courierId);
}