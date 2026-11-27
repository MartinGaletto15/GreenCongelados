using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(GCContext context) : base(context)
    {
    }

    public async Task<Product?> GetByNameAsync(string name)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task<IEnumerable<Product>> GetAllNotDeletedAsync()
    {
        return await _context.Products
            .Where(p => p.Status != ProductStatus.Deleted && p.Status != ProductStatus.Hidden)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdNotDeletedAsync(int id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.IdProduct == id && p.Status != ProductStatus.Deleted);
    }

    public async Task<Product?> GetByIdReadOnlyAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProduct == id);
    }

    public async Task<bool> TryDecreaseStockAsync(int idProduct, int quantity)
    {
        // Una sola sentencia. SQL Server primero bloquea la fila y recién después revisa si se cumple la condición:
        // si dos requests compiten, uno actualiza 1 fila y el otro 0.
        var affected = await _context.Products
            .Where(p => p.IdProduct == idProduct
                     && p.Status == ProductStatus.Available
                     && p.CurrentStock >= quantity)
            .ExecuteUpdateAsync(s => s.SetProperty(
                p => p.CurrentStock, p => p.CurrentStock - quantity));

        if (affected == 0)
            return false;

        // Misma definición que SyncStatusWithStock, pero en SQL.
        await _context.Products
            .Where(p => p.IdProduct == idProduct && p.CurrentStock <= 0)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, ProductStatus.OutOfStock));

        return true;
    }

    public async Task IncreaseStockAsync(int idProduct, int quantity)
    {
        await _context.Products
            .Where(p => p.IdProduct == idProduct)
            .ExecuteUpdateAsync(s => s.SetProperty(
                p => p.CurrentStock, p => p.CurrentStock + quantity));

        // Equivale a IncreaseStock: solo sincroniza si no estaba Hidden ni Deleted.
        await _context.Products
            .Where(p => p.IdProduct == idProduct
                     && (p.Status == ProductStatus.Available || p.Status == ProductStatus.OutOfStock))
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status,
                p => p.CurrentStock > 0 ? ProductStatus.Available : ProductStatus.OutOfStock));
    }
}
