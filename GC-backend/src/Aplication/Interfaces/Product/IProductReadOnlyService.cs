using Applications.dtos;

namespace Aplication.Interfaces.Product;

public interface IProductReadOnlyService
{
    Task<IEnumerable<ProductDTO>> GetAllAsync();
    Task<ProductDTO> GetByIdAsync(int id);
}
