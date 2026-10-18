using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests.Update;

public record UpdateCartItemRequest(
    [Required] int Quantity
);