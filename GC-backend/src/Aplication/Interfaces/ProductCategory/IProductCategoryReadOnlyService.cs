using Applications.dtos;

namespace Aplication.Interfaces.ProductCategory;

public interface IProductCategoryReadOnlyService
{
    Task<IEnumerable<CategoryDTO>> GetCategoriesByProductIdAsync(int productId);
}