using Applications.dtos;
using Applications.dtos.Requests;

namespace Aplication.Interfaces.Product;

public interface IProductWriteService
{
    Task<ProductDTO> CreateAsync(CreateProductRequest request);
    Task<ProductDTO> UpdateAsync(int id, UpdateProductRequest request);
    Task DeleteAsync(int id);
}
