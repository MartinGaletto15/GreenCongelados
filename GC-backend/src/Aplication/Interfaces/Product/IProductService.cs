using Applications.dtos;
using Applications.dtos.Requests;

namespace Aplication.Interfaces.Product;

public interface IProductService
{
    Task<IEnumerable<ProductDTO>> GetAllAsync();
    Task<ProductDTO> GetByIdAsync(int id);
    Task<ProductDTO> CreateAsync(CreateProductRequest request);
    Task<ProductDTO> UpdateAsync(int id, UpdateProductRequest request);
    Task DeleteAsync(int id);
}
