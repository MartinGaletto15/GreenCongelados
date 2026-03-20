using Domain.Entities;

namespace Domain.Interfaces;

public interface ICartItemRepository : IGenericRepository<CartItem>
{
    Task<IEnumerable<CartItem>> GetByUserIdAsync(int userId);
    Task<Cart?> GetUserCartAsync(int userId);
    Task<Cart> CreateCartAsync(int userId);
}