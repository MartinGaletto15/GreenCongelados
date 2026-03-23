using Aplication.Interfaces.Product;
using Applications.dtos;
using Applications.dtos.Requests;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;
using Aplication.Validators;

namespace Aplication.Services;

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
        var entities = await _repository.GetAllAsync();
        return ProductDTO.Create(entities);
    }

    public async Task<ProductDTO> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Product not found", "PRODUCT_NOT_FOUND");
        return ProductDTO.Create(entity);
    }

    public async Task<ProductDTO> CreateAsync(CreateProductRequest request)
    {
        await ProductValidator.ValidateCreateAsync(request, _repository);

        var entity = new Product
        {
            Name = request.Name,
            UrlImagePrimary = request.UrlImagePrimary,
            DescriptionShort = request.DescriptionShort,
            DescriptionLong = request.DescriptionLong,
            Price = request.Price,
            CurrentStock = request.CurrentStock,
            Weight = request.Weight,
            PreparationTime = request.PreparationTime,
            Status = request.Status
        };

        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return ProductDTO.Create(entity);
    }

    public async Task<ProductDTO> UpdateAsync(int id, UpdateProductRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Product not found", "PRODUCT_NOT_FOUND");

        await ProductValidator.ValidateUpdateAsync(id, request, _repository, entity);

        entity.Name = request.Name ?? entity.Name;
        entity.UrlImagePrimary = request.UrlImagePrimary ?? entity.UrlImagePrimary;
        entity.DescriptionShort = request.DescriptionShort ?? entity.DescriptionShort;
        entity.DescriptionLong = request.DescriptionLong ?? entity.DescriptionLong;
        entity.Price = request.Price ?? entity.Price;
        entity.CurrentStock = request.CurrentStock ?? entity.CurrentStock;
        entity.Weight = request.Weight ?? entity.Weight;
        entity.PreparationTime = request.PreparationTime ?? entity.PreparationTime;
        entity.Status = request.Status ?? entity.Status;

        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return ProductDTO.Create(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Product not found", "PRODUCT_NOT_FOUND");
        await _repository.DeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
