using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class ShippingCost
{
    [Key]
    public int IdShippingCost { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public required decimal Cost { get; set; }

    [Required]
    public required bool IsActive { get; set; }
}