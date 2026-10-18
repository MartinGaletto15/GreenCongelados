using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests.Create;

public record CreateCartItemRequest(
    [Required] int IdProduct,
    [Required] int Quantity
);