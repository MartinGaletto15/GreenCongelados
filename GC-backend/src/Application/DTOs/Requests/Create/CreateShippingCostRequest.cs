using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record CreateShippingCostRequest(
    [Required][MaxLength(50)] string Name,
    [Required] decimal Cost,
    [Required] bool IsActive
);
