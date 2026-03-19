using System.ComponentModel.DataAnnotations;

namespace Aplication.DTOs.Requests.Create;

public record CreateCartItemRequest(
    [Required] int IdProduct,
    [Required] int Quantity
);