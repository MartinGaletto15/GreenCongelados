using Aplication.Interfaces.ProductCategory;
using Applications.dtos;
using Applications.dtos.Requests;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;

namespace Aplication.Services;

public class ProductCategoryService : IProductCategoryService
{
    private readonly IGenericRepository<ProductCategory> _repository;

    public ProductCategoryService(IGenericRepository<ProductCategory> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProductCategoryDTO>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return ProductCategoryDTO.Create(entities);
    }

    public async Task<ProductCategoryDTO> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("ProductCategory not found", "PRODUCTCATEGORY_NOT_FOUND");
        return ProductCategoryDTO.Create(entity);
    }

    public async Task<ProductCategoryDTO> CreateAsync(CreateProductCategoryRequest request)
    {
        var entity = new ProductCategory
        {
            IdProduct = request.IdProduct,
            IdCategory = request.IdCategory
        };

        await _repository.AddAsync(entity);
        return ProductCategoryDTO.Create(entity);
    }

    public async Task<ProductCategoryDTO> UpdateAsync(int id, UpdateProductCategoryRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("ProductCategory not found", "PRODUCTCATEGORY_NOT_FOUND");

        entity.IdProduct = request.IdProduct ?? entity.IdProduct;
        entity.IdCategory = request.IdCategory ?? entity.IdCategory;

        await _repository.UpdateAsync(entity);
        return ProductCategoryDTO.Create(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("ProductCategory not found", "PRODUCTCATEGORY_NOT_FOUND");
        await _repository.DeleteAsync(entity);
    }
}