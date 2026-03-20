using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
{
    public CartItemRepository(GCContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CartItem>> GetByUserIdAsync(int userId)
    {
        return await _context.Set<CartItem>()
            .Include(ci => ci.Product)
            .Include(ci => ci.Cart)
            .Where(ci => ci.Cart.IdUser == userId)
            .ToListAsync();
    }

    public async Task<Cart?> GetUserCartAsync(int userId)
    {
        return await _context.Set<Cart>()
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.IdUser == userId);
    }

    public async Task<Cart> CreateCartAsync(int userId)
    {
        var cart = new Cart { IdUser = userId };
        await _context.Set<Cart>().AddAsync(cart);
        await _context.SaveChangesAsync();
        return cart;
    }
}