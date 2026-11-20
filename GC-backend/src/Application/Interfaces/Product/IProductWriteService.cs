using Application.DTOs;
using Application.DTOs.Requests;

namespace Application.Interfaces.Product;

public interface IProductWriteService
{
    Task<ProductDTO> CreateAsync(CreateProductRequest request);
    Task<ProductDTO> UpdateAsync(int id, UpdateProductRequest request);
    Task<ProductDTO> HideAsync(int id);
    Task<ProductDTO> PublishAsync(int id);
    Task DeleteAsync(int id);
}
