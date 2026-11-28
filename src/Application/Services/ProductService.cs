using Application.Interfaces.Product;
using Application.DTOs;
using Application.DTOs.Requests;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;

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
        var productName = request.Name.Trim();

        // La unicidad depende del repositorio, así que no puede vivir en la entidad.
        var existingProduct = await _repository.GetByNameAsync(productName);
        if (existingProduct != null)
            throw new AppValidationException("Ya existe un producto con ese nombre.", "PRODUCT_NAME_EXISTS", 409);

        var entity = new Product(
            productName,
            request.UrlImagePrimary,
            request.DescriptionShort,
            request.DescriptionLong,
            request.Price,
            request.CurrentStock,
            request.Weight,
            request.PreparationTime
        );

        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return ProductDTO.Create(entity);
    }

    public async Task<ProductDTO> UpdateAsync(int id, UpdateProductRequest request)
    {
        var entity = await _repository.GetByIdNotDeletedAsync(id);
        if (entity == null) throw new AppValidationException("Producto no encontrado.", "PRODUCT_NOT_FOUND", 404);

        var productName = request.Name?.Trim();
        if (productName != null && !string.Equals(entity.Name, productName, StringComparison.OrdinalIgnoreCase))
        {
            var existingProduct = await _repository.GetByNameAsync(productName);
            if (existingProduct != null && existingProduct.IdProduct != id)
                throw new AppValidationException("Ya existe un producto con ese nombre.", "PRODUCT_NAME_EXISTS", 409);
        }

        // Un null significa "no cambiar": la entidad recibe siempre el valor final.
        entity.UpdateInfo(
            productName ?? entity.Name,
            request.UrlImagePrimary ?? entity.UrlImagePrimary,
            request.DescriptionShort ?? entity.DescriptionShort,
            request.DescriptionLong ?? entity.DescriptionLong,
            request.Weight ?? entity.Weight,
            request.PreparationTime ?? entity.PreparationTime
        );

        // El precio y el stock tienen su propia regla de negocio, por eso no
        // viajan dentro de UpdateInfo.
        if (request.Price.HasValue)
            entity.UpdatePrice(request.Price.Value);

        if (request.CurrentStock.HasValue)
            entity.AdjustStock(request.CurrentStock.Value);

        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return ProductDTO.Create(entity);
    }

    public async Task<ProductDTO> HideAsync(int id)
    {
        var entity = await _repository.GetByIdNotDeletedAsync(id);
        if (entity == null) throw new AppValidationException("Producto no encontrado.", "PRODUCT_NOT_FOUND", 404);

        entity.Hide();
        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return ProductDTO.Create(entity);
    }

    public async Task<ProductDTO> PublishAsync(int id)
    {
        var entity = await _repository.GetByIdNotDeletedAsync(id);
        if (entity == null) throw new AppValidationException("Producto no encontrado.", "PRODUCT_NOT_FOUND", 404);

        entity.Publish();
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
