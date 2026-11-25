using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(GCContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetAllWithDetailsAsync()
    {
        return await _context.Orders
            .Include(o => o.Courier)
            .Include(o => o.Promotion)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Courier)
            .Include(o => o.Promotion)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.IdOrder == id);
    }

    public async Task<Order?> GetByIdWithDetailsNoTrackingAsync(int idOrder)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Courier)
            .Include(o => o.Promotion)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.IdOrder == idOrder);
    }

    public async Task<IEnumerable<Order>> GetByUserIdWithDetailsAsync(int userId)
    {
        return await _context.Orders
            .Include(o => o.Courier)
            .Include(o => o.Promotion)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.IdUser == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByCourierIdWithDetailsAsync(int courierId)
    {
        return await _context.Orders
            .Include(o => o.Courier)
            .Include(o => o.Promotion)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.IdCourier == courierId && o.OrderStatus != OrderStatus.Cancelled)
            .ToListAsync();
    }

    public async Task<bool> TryCancelAsync(int idOrder)
    {
        // Solo el request que mueve la fila devuelve stock.
        var affected = await _context.Orders
            .Where(o => o.IdOrder == idOrder
                     && o.OrderStatus != OrderStatus.Cancelled
                     && o.OrderStatus != OrderStatus.Delivered)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.OrderStatus, OrderStatus.Cancelled));

        return affected == 1;
    }
}
