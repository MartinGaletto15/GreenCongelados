using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record CreateOrderItemRequest(
    [Required(ErrorMessage = "La orden es obligatoria.")] int IdOrder,
    [Required(ErrorMessage = "El producto es obligatorio.")] int IdProduct,
    [Required(ErrorMessage = "La cantidad es obligatoria.")] int Quantity,
    [Required(ErrorMessage = "El precio unitario es obligatorio.")] decimal UnitPrice
);
