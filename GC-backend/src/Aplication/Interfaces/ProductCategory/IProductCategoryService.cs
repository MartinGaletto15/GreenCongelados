using Applications.dtos;
using Applications.dtos.Requests;

namespace Aplication.Interfaces.ProductCategory;

public interface IProductCategoryService
{
    Task<IEnumerable<ProductCategoryDTO>> GetAllAsync();
    Task<ProductCategoryDTO> GetByIdAsync(int id);
    Task<ProductCategoryDTO> CreateAsync(CreateProductCategoryRequest request);
    Task<ProductCategoryDTO> UpdateAsync(int id, UpdateProductCategoryRequest request);
    Task DeleteAsync(int id);
}
