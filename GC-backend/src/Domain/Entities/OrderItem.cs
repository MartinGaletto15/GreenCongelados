using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Exceptions;

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

    protected OrderItem() { }

    public OrderItem(int idProduct, int quantity, decimal unitPrice, int idOrder = 0)
    {
        if (quantity <= 0) throw new AppValidationException("La cantidad debe ser mayor que cero.", "INVALID_QUANTITY");
        if (unitPrice < 0) throw new AppValidationException("El precio unitario no puede ser negativo.", "INVALID_PRICE");

        IdOrder = idOrder;
        IdProduct = idProduct;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
