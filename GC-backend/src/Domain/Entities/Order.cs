using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums.Entities;

namespace Domain.Entities;

public class Order
{
    [Key]
    public int IdOrder { get; set; }

    [Required]
    public required int IdUser { get; set; }
    public User? User { get; set; }

    public int? IdCourier { get; set; }
    public User? Courier { get; set; }

    public int? IdPromotion { get; set; }
    public Promotion? Promotion { get; set; }

    [Required] 
    [Column(TypeName = "decimal(18, 2)")]
    public decimal ShippingCost { get; set; }

    [Required] 
    public required string ShippingStreet { get; set; }

    public string? ShippingDpto { get; set; }

    public string? ShippingReference { get; set; }

    [Required]
    public required DateTime OrderDate { get; set; }

    public decimal GlobalDiscount { get; set; } = 0;

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public required decimal Subtotal { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public required decimal Total { get; set; }

    [Required]
    public required OrderStatus OrderStatus { get; set; } 
    
    // Navigation property
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}