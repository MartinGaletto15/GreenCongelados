using Domain.Entities;

namespace Domain.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<IEnumerable<Order>> GetAllWithDetailsAsync();
    Task<Order?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Order>> GetByUserIdWithDetailsAsync(int userId);
    Task<IEnumerable<Order>> GetByCourierIdWithDetailsAsync(int courierId);
}
