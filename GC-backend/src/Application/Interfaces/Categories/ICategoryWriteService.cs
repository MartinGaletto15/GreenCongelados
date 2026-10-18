using Application.DTOs;
using Application.DTOs.Requests;

namespace Application.Interfaces.Categories;

public interface ICategoryWriteService
{
    Task<CategoryDTO> CreateAsync(CreateCategoryRequest request);
    Task<CategoryDTO> UpdateAsync(int id, UpdateCategoryRequest request);
    Task DeleteAsync(int id);
}