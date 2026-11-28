using Application.DTOs;

namespace Application.Interfaces.Categories;

public interface ICategoryReadOnlyService
{
    Task<IEnumerable<CategoryDTO>> GetAllAsync();
    Task<CategoryDTO> GetByIdAsync(int id);
}