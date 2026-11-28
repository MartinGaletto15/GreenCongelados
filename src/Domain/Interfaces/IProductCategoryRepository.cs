using Domain.Entities;

namespace Domain.Interfaces;

public interface IProductCategoryRepository : IGenericRepository<ProductCategory>
{
    Task<IEnumerable<Category>> GetCategoriesByProductIdAsync(int productId);
    Task<ProductCategory?> GetByIdsAsync(int productId, int categoryId);
    Task<IEnumerable<ProductCategory>> GetByProductIdAsync(int productId);
    Task DeleteRangeAsync(IEnumerable<ProductCategory> entities);
}