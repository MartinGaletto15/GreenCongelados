using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests.Update;

public record UpdateCartItemRequest(
    [Required(ErrorMessage = "La cantidad es obligatoria.")] int Quantity
);