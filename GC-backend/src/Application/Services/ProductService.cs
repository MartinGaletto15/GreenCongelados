using Application.Interfaces.Product;
using Application.DTOs;
using Application.DTOs.Requests;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;
using Application.Validators;

namespace Application.Services;

public class ProductService : IProductReadOnlyService, IProductWriteService
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProductDTO>> GetAllAsync()
    {
        var entities = await _repository.GetAllNotDeletedAsync();
        return ProductDTO.Create(entities);
    }

    public async Task<ProductDTO> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdNotDeletedAsync(id);
        if (entity == null)
            throw new AppValidationException("Producto no encontrado.", "PRODUCT_NOT_FOUND", 404);
        return ProductDTO.Create(entity);
    }

    public async Task<ProductDTO> CreateAsync(CreateProductRequest request)
    {
        await ProductValidator.ValidateCreateAsync(request, _repository);

        var entity = new Product(
            request.Name,
            request.UrlImagePrimary,
            request.DescriptionShort,
            request.DescriptionLong,
            request.Price,
            request.CurrentStock,
            request.Weight,
            request.PreparationTime,
            request.Status
        );

        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return ProductDTO.Create(entity);
    }

    public async Task<ProductDTO> UpdateAsync(int id, UpdateProductRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Producto no encontrado.", "PRODUCT_NOT_FOUND", 404);

        await ProductValidator.ValidateUpdateAsync(id, request, _repository, entity);

        entity.UpdateDetails(
            request.Name,
            request.UrlImagePrimary,
            request.DescriptionShort,
            request.DescriptionLong,
            request.Price,
            request.CurrentStock,
            request.Weight,
            request.PreparationTime,
            request.Status
        );

        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return ProductDTO.Create(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Producto no encontrado.", "PRODUCT_NOT_FOUND", 404);
        
        entity.Delete();
        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
