using Applications.dtos.Requests;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Aplication.Validators;

public static class ProductValidator
{
    public static async Task ValidateCreateAsync(CreateProductRequest request, IProductRepository repository)
    {
        if (!Validations.ValidateString(request.Name, 2, 100))
            throw new AppValidationException("El nombre es obligatorio y debe tener entre 2 y 100 caracteres.", "PRODUCT_NAME_INVALID");

        if (!Validations.ValidateString(request.DescriptionShort, 2, 255))
            throw new AppValidationException("La descripción corta es obligatoria y debe tener entre 2 y 255 caracteres.", "PRODUCT_DESCRIPTION_INVALID");

        if (request.Price <= 0)
            throw new AppValidationException("El precio debe ser mayor a 0.", "PRODUCT_PRICE_INVALID");

        if (request.CurrentStock < 0)
            throw new AppValidationException("El stock no puede ser negativo.", "PRODUCT_STOCK_INVALID");

        if (request.PreparationTime < 0)
            throw new AppValidationException("El tiempo de preparación no puede ser negativo.", "PRODUCT_PREPARATION_TIME_INVALID");

        var existingProduct = await repository.GetByNameAsync(request.Name);
        if (existingProduct != null)
            throw new AppValidationException("Ya existe un producto con ese nombre.", "PRODUCT_NAME_EXISTS");
    }

    public static async Task ValidateUpdateAsync(int id, UpdateProductRequest request, IProductRepository repository, Domain.Entities.Product currentEntity)
    {
        if (request.Name != null && !Validations.ValidateString(request.Name, 2, 100))
            throw new AppValidationException("El nombre debe tener entre 2 y 100 caracteres.", "PRODUCT_NAME_INVALID");

        if (request.DescriptionShort != null && !Validations.ValidateString(request.DescriptionShort, 2, 255))
            throw new AppValidationException("La descripción corta debe tener entre 2 y 255 caracteres.", "PRODUCT_DESCRIPTION_INVALID");

        if (request.Price.HasValue && request.Price <= 0)
            throw new AppValidationException("El precio debe ser mayor a 0.", "PRODUCT_PRICE_INVALID");

        if (request.CurrentStock.HasValue && request.CurrentStock < 0)
            throw new AppValidationException("El stock no puede ser negativo.", "PRODUCT_STOCK_INVALID");

        if (request.PreparationTime.HasValue && request.PreparationTime < 0)
            throw new AppValidationException("El tiempo de preparación no puede ser negativo.", "PRODUCT_PREPARATION_TIME_INVALID");

        if (request.Name != null && currentEntity.Name != request.Name)
        {
            var existingProduct = await repository.GetByNameAsync(request.Name);
            if (existingProduct != null)
                throw new AppValidationException("Ya existe un producto con ese nombre.", "PRODUCT_NAME_EXISTS");
        }
    }
}
