using Applications.dtos;
using Applications.dtos.Requests;

namespace Aplication.Interfaces.Categories;

public interface ICategoryWriteService
{
    Task<CategoryDTO> CreateAsync(CreateCategoryRequest request);
    Task<CategoryDTO> UpdateAsync(int id, UpdateCategoryRequest request);
    Task DeleteAsync(int id);
}