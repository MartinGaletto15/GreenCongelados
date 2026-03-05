using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class OrderItem
{
    // Parte de la clave primaria (PK) compuesta y FK a Order
    [Required]
    public required int IdOrder { get; set; }
    public required Order Order { get; set; }

    // Parte de la clave primaria (PK) compuesta y FK a Product
    [Required]
    public required int IdProduct { get; set; }
    public required Product Product { get; set; }   

    [Required]
    public required int Quantity { get; set; }
    
    [Required]
    public required decimal UnitPrice { get; set; }
}