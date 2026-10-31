using Application.Interfaces.Categories;
using Application.DTOs;
using Application.DTOs.Requests;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;

namespace Application.Services;

public class CategoryService : ICategoryReadOnlyService, ICategoryWriteService
{
    private readonly IGenericRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IGenericRepository<Category> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return CategoryDTO.Create(entities);
    }

    public async Task<CategoryDTO> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Categoría no encontrada.", "CATEGORY_NOT_FOUND", 404);
        return CategoryDTO.Create(entity);
    }

    public async Task<CategoryDTO> CreateAsync(CreateCategoryRequest request)
    {
        var entity = new Category
        {
            Name = request.Name,
            ImageUrl = request.ImageUrl
        };

        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return CategoryDTO.Create(entity);
    }

    public async Task<CategoryDTO> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Categoría no encontrada.", "CATEGORY_NOT_FOUND", 404);

        entity.Name = request.Name ?? entity.Name;
        entity.ImageUrl = request.ImageUrl ?? entity.ImageUrl;

        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return CategoryDTO.Create(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Categoría no encontrada.", "CATEGORY_NOT_FOUND", 404);
        await _repository.DeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
