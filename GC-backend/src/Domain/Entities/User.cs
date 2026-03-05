using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Domain.Entities;
public class User
{
    [Key]
    public int IdUser { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(50)]
    public required string LastName { get; set; }

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Password { get; set; }

    [Required]
    [MaxLength(25)]
    public string? Phone { get; set; }

    [Required]
    public required Role Role { get; set; }

    [Required]
    public required bool IsActive { get; set; }


    // Órdenes que el usuario HIZO (como Cliente)
    [InverseProperty("User")] 
    public ICollection<Order> OrdersPlaced { get; set; } = new List<Order>();

    // Órdenes que el usuario ENTREGA (como Cadete)
    [InverseProperty("Courier")] 
    public ICollection<Order> OrdersToDeliver { get; set; } = new List<Order>();
}