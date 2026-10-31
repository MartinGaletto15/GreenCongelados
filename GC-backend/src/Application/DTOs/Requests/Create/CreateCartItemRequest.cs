using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests.Create;

public record CreateCartItemRequest(
    [Required(ErrorMessage = "El producto es obligatorio.")] int IdProduct,
    [Required(ErrorMessage = "La cantidad es obligatoria.")] int Quantity
);