using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("Product_Category")]
public class ProductCategory
{
    [Key, Column(Order = 0)]
    public int IdProduct { get; set; }
    public Product Product { get; set; } = null!;

    [Key, Column(Order = 1)]
    public int IdCategory { get; set; }
    public Category Category { get; set; } = null!;
}