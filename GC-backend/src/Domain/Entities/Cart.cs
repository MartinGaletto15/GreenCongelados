using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Cart
{
    [Key]
    public int IdCart { get; set; }

    [Required]
    public int IdUser { get; set; }

    [ForeignKey("IdUser")]
    public User User { get; set; } = null!;

    // Navigation property
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}