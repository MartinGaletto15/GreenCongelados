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

    public decimal? PreparationTime { get; private set; }

    [MaxLength(25)]
    [Column("product_status")]
    public ProductStatus Status { get; private set; } = ProductStatus.Available;

    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();
    public ICollection<ProductCategory> ProductCategories { get; private set; } = new List<ProductCategory>();
    public ICollection<CartItem> CartItems { get; private set; } = new List<CartItem>();

    // EF constructor
    protected Product() {
        Name = string.Empty;
        DescriptionShort = string.Empty;
    }

    public Product(string name, string? urlImagePrimary, string descriptionShort, string? descriptionLong, decimal price, int currentStock, decimal? weight, decimal? preparationTime, ProductStatus status = ProductStatus.Available)
    {
        ValidateProductData(name, price, currentStock);

        Name = name;
        UrlImagePrimary = urlImagePrimary;
        DescriptionShort = descriptionShort;
        DescriptionLong = descriptionLong;
        Price = price;
        CurrentStock = currentStock;
        Weight = weight;
        PreparationTime = preparationTime;
        Status = status;
    }

    public void UpdateDetails(string? name, string? urlImagePrimary, string? descriptionShort, string? descriptionLong, decimal? price, int? currentStock, decimal? weight, decimal? preparationTime, ProductStatus? status)
    {
        if (name != null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new AppValidationException("Name cannot be empty", "INVALID_NAME");
            Name = name;
        }

        if (price.HasValue)
        {
            if (price.Value < 0) throw new AppValidationException("Price cannot be negative", "INVALID_PRICE");
            Price = price.Value;
        }

        if (currentStock.HasValue)
        {
            if (currentStock.Value < 0) throw new AppValidationException("Stock cannot be negative", "INVALID_STOCK");
            
            // If stock was 0 (OutOfStock) and now it's > 0, we change status to Available
            if (CurrentStock == 0 && currentStock.Value > 0 && Status == ProductStatus.OutOfStock)
            {
                Status = ProductStatus.Available;
            }
            // If stock was > 0 and now it's 0, we change status to OutOfStock
            else if (currentStock.Value == 0 && Status == ProductStatus.Available)
            {
                Status = ProductStatus.OutOfStock;
            }

            CurrentStock = currentStock.Value;
        }

        UrlImagePrimary = urlImagePrimary ?? UrlImagePrimary;
        DescriptionShort = descriptionShort ?? DescriptionShort;
        DescriptionLong = descriptionLong ?? DescriptionLong;
        Weight = weight ?? Weight;
        PreparationTime = preparationTime ?? PreparationTime;
        Status = status ?? Status;
    }

    public void ReduceStock(int quantity)
    {
        if (quantity <= 0) throw new AppValidationException("Quantity must be positive", "INVALID_QUANTITY");
        if (CurrentStock < quantity) throw new AppValidationException($"Insufficient stock for product: {Name}. Available: {CurrentStock}", "INSUFFICIENT_STOCK");

        CurrentStock -= quantity;
        
        if (CurrentStock == 0)
        {
            Status = ProductStatus.OutOfStock;
        }
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0) throw new AppValidationException("Quantity must be positive", "INVALID_QUANTITY");
        CurrentStock += quantity;

        if (CurrentStock > 0 && Status == ProductStatus.OutOfStock)
        {
            Status = ProductStatus.Available;
        }
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0) throw new AppValidationException("Price cannot be negative", "INVALID_PRICE");
        Price = newPrice;
    }

    public void Delete()
    {
        Status = ProductStatus.Deleted;
    }

    private void ValidateProductData(string name, decimal price, int currentStock)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new AppValidationException("Name cannot be empty", "INVALID_NAME");
        if (price < 0) throw new AppValidationException("Price cannot be negative", "INVALID_PRICE");
        if (currentStock < 0) throw new AppValidationException("Stock cannot be negative", "INVALID_STOCK");
    }
}
