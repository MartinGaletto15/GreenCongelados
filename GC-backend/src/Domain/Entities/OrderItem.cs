using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class OrderItem
{
    [Key]
    public int IdOrderItem { get; private set; }

    [Required]
    public int IdOrder { get; private set; }
    public Order? Order { get; private set; }

    [Required]
    public int IdProduct { get; private set; }
    public Product? Product { get; private set; }   

    [Required]
    public int Quantity { get; private set; }
    
    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; private set; }

    // EF constructor
    protected OrderItem() { }

    public OrderItem(int idProduct, int quantity, decimal unitPrice, int idOrder = 0)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        if (unitPrice < 0) throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        IdOrder = idOrder;
        IdProduct = idProduct;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
