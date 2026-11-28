using Application.DTOs;

namespace Application.Interfaces.Product;

public interface IProductReadOnlyService
{
    Task<IEnumerable<ProductDTO>> GetAllAsync();
    Task<ProductDTO> GetByIdAsync(int id);
}
