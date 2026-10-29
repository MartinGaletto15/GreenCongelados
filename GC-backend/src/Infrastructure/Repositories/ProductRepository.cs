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
            .Where(p => p.Status != ProductStatus.Deleted)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdNotDeletedAsync(int id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.IdProduct == id && p.Status != ProductStatus.Deleted);
    }
}
