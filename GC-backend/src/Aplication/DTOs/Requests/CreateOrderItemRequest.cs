using System.ComponentModel.DataAnnotations;

namespace Applications.dtos.Requests;

public record CreateOrderItemRequest(
    [Required] int IdOrder,
    [Required] int IdProduct,
    [Required] int Quantity,
    [Required] decimal UnitPrice
);
