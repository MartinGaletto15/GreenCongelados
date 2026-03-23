using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Product
{
    [Key]
    public int IdProduct { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? UrlImagePrimary { get; set; }

    [Required]
    [MaxLength(255)]
    public required string DescriptionShort { get; set; }

    public string? DescriptionLong { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")] // Define precision and scale for money
    public required decimal Price { get; set; }

    [Required]
    public required int CurrentStock { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Weight { get; set; }

    public decimal? PreparationTime { get; set; }

    [MaxLength(25)]
    [Column("product_status")]
    public ProductStatus Status { get; set; } = ProductStatus.Available;

    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}