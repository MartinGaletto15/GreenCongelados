using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record UpdateShippingCostRequest(
    [MaxLength(50)] string? Name = null,
    decimal? Cost = null,
    bool? IsActive = null
);
