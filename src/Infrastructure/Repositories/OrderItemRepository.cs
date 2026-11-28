using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(GCContext context) : base(context)
    {
    }

    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
    {
        return await _context.Set<OrderItem>()
            .Include(oi => oi.Product)
            .Where(oi => oi.IdOrder == orderId)
            .ToListAsync();
    }
}
