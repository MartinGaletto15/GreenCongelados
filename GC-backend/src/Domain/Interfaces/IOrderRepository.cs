using Domain.Entities;

namespace Domain.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<IEnumerable<Order>> GetAllWithDetailsAsync();
    Task<Order?> GetByIdWithDetailsAsync(int id);
    Task<Order?> GetByIdWithDetailsNoTrackingAsync(int idOrder);
    Task<IEnumerable<Order>> GetByUserIdWithDetailsAsync(int userId);
    Task<IEnumerable<Order>> GetByCourierIdWithDetailsAsync(int courierId);
    Task<bool> TryCancelAsync(int idOrder);
}
