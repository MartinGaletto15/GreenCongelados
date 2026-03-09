using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Category
{
    [Key]
    public int IdCategory { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    [Required]
    public required string ImageUrl { get; set; }

    // Navigation property
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}