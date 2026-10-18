using Application.DTOs;
using Application.DTOs.Requests;

namespace Application.Interfaces.Product;

public interface IProductWriteService
{
    Task<ProductDTO> CreateAsync(CreateProductRequest request);
    Task<ProductDTO> UpdateAsync(int id, UpdateProductRequest request);
    Task DeleteAsync(int id);
}
