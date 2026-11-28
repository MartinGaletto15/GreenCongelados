using Application.Interfaces.ProductCategory;
using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;

namespace Application.Services;

public class ProductCategoryService : IProductCategoryReadOnlyService, IProductCategoryWriteService
{
    private readonly IProductCategoryRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductCategoryService(
        IProductCategoryRepository repository,
        IProductRepository productRepository,
        IGenericRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CategoryDTO>> GetCategoriesByProductIdAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) throw new AppValidationException("Producto no encontrado.", "PRODUCT_NOT_FOUND", 404);

        var categories = await _repository.GetCategoriesByProductIdAsync(productId);
        return CategoryDTO.Create(categories);
    }

    public async Task AddCategoryToProductAsync(int productId, int categoryId)
    {
        // Check existence
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) throw new AppValidationException("Producto no encontrado.", "PRODUCT_NOT_FOUND", 404);

        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null) throw new AppValidationException("Categoría no encontrada.", "CATEGORY_NOT_FOUND", 404);

        // Check for conflict (duplicate)
        var existing = await _repository.GetByIdsAsync(productId, categoryId);
        if (existing != null) throw new AppValidationException("El producto ya está asociado a esta categoría.", "PRODUCT_CATEGORY_CONFLICT", 409);

        var entity = new ProductCategory
        {
            IdProduct = productId,
            IdCategory = categoryId
        };

        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveCategoryFromProductAsync(int productId, int categoryId)
    {
        var existing = await _repository.GetByIdsAsync(productId, categoryId);
        if (existing == null) throw new AppValidationException("La categoría no está asociada a este producto.", "PRODUCT_CATEGORY_NOT_FOUND", 404);

        await _repository.DeleteAsync(existing);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SyncCategoriesToProductAsync(int productId, IEnumerable<int> categoryIds)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) throw new AppValidationException("Producto no encontrado.", "PRODUCT_NOT_FOUND", 404);

        // Remove all current
        var current = await _repository.GetByProductIdAsync(productId);
        if (current.Any())
        {
            await _repository.DeleteRangeAsync(current);
        }

        // Add new ones
        foreach (var categoryId in categoryIds)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null) throw new AppValidationException($"Categoría con ID {categoryId} no encontrada.", "CATEGORY_NOT_FOUND", 404);

            await _repository.AddAsync(new ProductCategory
            {
                IdProduct = productId,
                IdCategory = categoryId
            });
        }
        await _unitOfWork.SaveChangesAsync();
    }
}