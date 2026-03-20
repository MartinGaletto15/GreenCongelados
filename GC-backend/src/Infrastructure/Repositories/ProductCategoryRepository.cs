using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductCategoryRepository : GenericRepository<ProductCategory>, IProductCategoryRepository
{
    public ProductCategoryRepository(GCContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetCategoriesByProductIdAsync(int productId)
    {
        return await _context.ProductCategories
            .Where(pc => pc.IdProduct == productId)
            .Select(pc => pc.Category)
            .ToListAsync();
    }

    public async Task<ProductCategory?> GetByIdsAsync(int productId, int categoryId)
    {
        return await _context.ProductCategories
            .FirstOrDefaultAsync(pc => pc.IdProduct == productId && pc.IdCategory == categoryId);
    }

    public async Task<IEnumerable<ProductCategory>> GetByProductIdAsync(int productId)
    {
        return await _context.ProductCategories
            .Where(pc => pc.IdProduct == productId)
            .ToListAsync();
    }

    public async Task DeleteRangeAsync(IEnumerable<ProductCategory> entities)
    {
        _context.ProductCategories.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }
}