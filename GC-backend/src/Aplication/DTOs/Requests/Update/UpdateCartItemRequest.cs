using System.ComponentModel.DataAnnotations;

namespace Aplication.DTOs.Requests.Update;

public record UpdateCartItemRequest(
    [Required] int Quantity
);