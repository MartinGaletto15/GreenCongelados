using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("Cart_Item")]
public class CartItem
{
    [Key]
    public int IdCartItem { get; set; }

    [Required]
    public int IdCart { get; set; }

    [ForeignKey("IdCart")]
    public Cart Cart { get; set; } = null!;

    [Required]
    public int IdProduct { get; set; }

    [ForeignKey("IdProduct")]
    public Product Product { get; set; } = null!;

    [Required]
    public int Quantity { get; set; }
}