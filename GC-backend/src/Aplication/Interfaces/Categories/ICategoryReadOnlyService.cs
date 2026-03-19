using Applications.dtos;

namespace Aplication.Interfaces.Categories;

public interface ICategoryReadOnlyService
{
    Task<IEnumerable<CategoryDTO>> GetAllAsync();
    Task<CategoryDTO> GetByIdAsync(int id);
}