using Applications.dtos;

namespace Aplication.Interfaces.Order;

public interface IOrderReadOnlyService
{
    Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
    Task<OrderDTO> GetOrderByIdAsync(int id, int userId, string role);
    Task<IEnumerable<OrderDTO>> GetMyOrdersAsync(int userId);
    Task<OrderDTO> GetMyOrderByIdAsync(int id, int userId, string role);
}