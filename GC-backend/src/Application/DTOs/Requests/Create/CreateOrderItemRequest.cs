using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record CreateOrderItemRequest(
    [Required] int IdOrder,
    [Required] int IdProduct,
    [Required] int Quantity,
    [Required] decimal UnitPrice
);
