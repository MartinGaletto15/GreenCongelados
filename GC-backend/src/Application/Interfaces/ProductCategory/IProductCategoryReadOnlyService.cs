using Application.DTOs;

namespace Application.Interfaces.ProductCategory;

public interface IProductCategoryReadOnlyService
{
    Task<IEnumerable<CategoryDTO>> GetCategoriesByProductIdAsync(int productId);
}