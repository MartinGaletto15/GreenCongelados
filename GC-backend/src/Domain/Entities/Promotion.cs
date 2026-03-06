using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums.Entities;

namespace Domain.Entities;

public class Promotion
{
    [Key]
    public int IdPromotion { get; set; }

    [Required]
    [MinLength(10)]
    [MaxLength(15)]
    public required string CouponCode { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public required decimal DiscountValue { get; set; }

    [Required]
    public required DiscountType DiscountType { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MinAmount { get; set; }

    [Required]
    public required DateTime StartDate { get; set; }

    [Required]
    public required DateTime EndDate { get; set; }

    // Propiedad de Navegación de Colección (Relación 1:N con Order)
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}