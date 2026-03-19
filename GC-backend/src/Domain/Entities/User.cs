using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    
    [MaxLength(25)]
    public string? Phone { get; set; }

    public Role Role { get; set; } = Role.USER;


    // Orders the user PLACED (as Client)
    [InverseProperty("User")] 
    public ICollection<Order> OrdersPlaced { get; set; } = new List<Order>();

    // Orders the user DELIVERS (as Courier)
    [InverseProperty("Courier")] 
    public ICollection<Order> OrdersToDeliver { get; set; } = new List<Order>();

    // Cart belonging to the user
    public Cart? Cart { get; set; }
}