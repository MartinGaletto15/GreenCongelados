using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class Product
{
    [Key]
    public int IdProduct { get; private set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; private set; }

    [MaxLength(200)]
    public string? UrlImagePrimary { get; private set; }

    [Required]
    [MaxLength(255)]
    public string DescriptionShort { get; private set; }

    public string? DescriptionLong { get; private set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; private set; }

    [Required]
    public int CurrentStock { get; private set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Weight { get; private set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? PreparationTime { get; private set; }

    [MaxLength(25)]
    [Column("product_status")]
    public ProductStatus Status { get; private set; } = ProductStatus.Available;

    public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();
    public ICollection<ProductCategory> ProductCategories { get; private set; } = new List<ProductCategory>();
    public ICollection<CartItem> CartItems { get; private set; } = new List<CartItem>();

    protected Product() {
        Name = string.Empty;
        DescriptionShort = string.Empty;
    }

    public Product(string name, string? urlImagePrimary, string descriptionShort, string? descriptionLong, decimal price, int currentStock, decimal? weight, decimal? preparationTime)
    {
        Name = ValidateName(name);
        DescriptionShort = ValidateDescriptionShort(descriptionShort);
        UrlImagePrimary = ValidateUrlImage(urlImagePrimary);
        ValidatePrice(price);
        ValidateStock(currentStock);
        ValidateWeight(weight);
        ValidatePreparationTime(preparationTime);

        DescriptionLong = descriptionLong;
        Price = price;
        CurrentStock = currentStock;
        Weight = weight;
        PreparationTime = preparationTime;

        // El estado inicial se deriva del stock: nadie de afuera lo elige.
        SyncStatusWithStock();
    }

    /// Datos descriptivos: no tocan precio ni stock.
    public void UpdateInfo(string name, string? urlImagePrimary, string descriptionShort, string? descriptionLong, decimal? weight, decimal? preparationTime)
    {
        EnsureNotDeleted();

        Name = ValidateName(name);
        DescriptionShort = ValidateDescriptionShort(descriptionShort);
        UrlImagePrimary = ValidateUrlImage(urlImagePrimary);
        ValidateWeight(weight);
        ValidatePreparationTime(preparationTime);

        DescriptionLong = descriptionLong;
        Weight = weight;
        PreparationTime = preparationTime;
    }

    /// Cambia el precio. No se regala: debe ser estrictamente mayor a 0.
    public void UpdatePrice(decimal newPrice)
    {
        EnsureNotDeleted();
        ValidatePrice(newPrice);

        Price = newPrice;
    }

    /// Ajusta el stock desde una edición manual. El estado se deriva del stock,
    /// salvo que el producto esté oculto: oculto se queda oculto hasta Publish().
    public void AdjustStock(int newStock)
    {
        EnsureNotDeleted();
        ValidateStock(newStock);

        CurrentStock = newStock;

        if (Status == ProductStatus.Hidden)
            return;

        SyncStatusWithStock();
    }

    public void Hide()
    {
        EnsureNotDeleted();
        Status = ProductStatus.Hidden;
    }

    /// Vuelve a mostrar el producto: el estado real depende del stock actual.
    public void Publish()
    {
        EnsureNotDeleted();
        SyncStatusWithStock();
    }

    /// Bloqueo permanente: un producto eliminado no vuelve a publicarse.
    public void Delete()
    {
        if (Status == ProductStatus.Deleted)
            return;

        Status = ProductStatus.Deleted;
    }

    public void EnsureAvailableFor(int quantity, int statusCode = 400)
    {
        if (quantity <= 0) throw new AppValidationException("La cantidad debe ser mayor que cero.", "INVALID_QUANTITY");
        if (Status != ProductStatus.Available) throw new AppValidationException($"El producto no está disponible: {Name}.", "PRODUCT_NOT_AVAILABLE", statusCode);
        if (CurrentStock < quantity) throw new AppValidationException($"Stock insuficiente para el producto: {Name}. Disponible: {CurrentStock}.", "INSUFFICIENT_STOCK", statusCode);
    }

    public void ReduceStock(int quantity)
    {
        EnsureAvailableFor(quantity);

        CurrentStock -= quantity;
        SyncStatusWithStock();
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0) throw new AppValidationException("La cantidad debe ser mayor que cero.", "INVALID_QUANTITY");

        CurrentStock += quantity;

        // Si estaba oculto o eliminado, sigue así: el stock no cambia esa decisión.
        if (Status is ProductStatus.Available or ProductStatus.OutOfStock)
        {
            SyncStatusWithStock();
        }
    }

    private void EnsureNotDeleted()
    {
        if (Status == ProductStatus.Deleted)
            throw new AppValidationException("El producto está eliminado y no se puede modificar.", "PRODUCT_DELETED", 409);
    }

    /// Única definición de la regla stock ↔ estado. Hidden y Deleted quedan
    /// fuera a propósito: no dependen del stock.
    private void SyncStatusWithStock()
    {
        Status = CurrentStock > 0 ? ProductStatus.Available : ProductStatus.OutOfStock;
    }

    private static string ValidateName(string? name)
    {
        if (!ValidateString(name, 2, 100))
            throw new AppValidationException("El nombre es obligatorio y debe tener entre 2 y 100 caracteres.", "INVALID_NAME");

        return name!.Trim();
    }

    private static string ValidateDescriptionShort(string? descriptionShort)
    {
        if (!ValidateString(descriptionShort, 2, 255))
            throw new AppValidationException("La descripción corta es obligatoria y debe tener entre 2 y 255 caracteres.", "INVALID_DESCRIPTION");

        return descriptionShort!.Trim();
    }

    private static string? ValidateUrlImage(string? urlImage)
    {
        if (urlImage is not null && urlImage.Length > 200)
            throw new AppValidationException("La URL de la imagen no puede superar los 200 caracteres.", "INVALID_IMAGE_URL");

        return urlImage;
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
            throw new AppValidationException("El precio debe ser mayor a 0.", "INVALID_PRICE");
    }

    private static void ValidateStock(int currentStock)
    {
        if (currentStock < 0)
            throw new AppValidationException("El stock no puede ser negativo.", "INVALID_STOCK");
    }

    private static void ValidateWeight(decimal? weight)
    {
        if (weight < 0)
            throw new AppValidationException("El peso no puede ser negativo.", "INVALID_WEIGHT");
    }

    private static void ValidatePreparationTime(decimal? preparationTime)
    {
        if (preparationTime < 0)
            throw new AppValidationException("El tiempo de preparación no puede ser negativo.", "INVALID_PREPARATION_TIME");
    }

    private static bool ValidateString(string? str, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(str))
            return false;

        string value = str.Trim();

        if (minLength > 0 && value.Length < minLength)
            return false;

        if (maxLength > 0 && value.Length > maxLength)
            return false;

        return true;
    }
}
