using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class OrderItem
{
    [Key]
    public int IdOrderItem { get; set; }

    [Required]
    public required int IdOrder { get; set; }
    public Order? Order { get; set; }
    [Required]
    public required int IdProduct { get; set; }
    public Product? Product { get; set; }   

    [Required]
    public required int Quantity { get; set; }
    
    [Required]
    public required decimal UnitPrice { get; set; }
}