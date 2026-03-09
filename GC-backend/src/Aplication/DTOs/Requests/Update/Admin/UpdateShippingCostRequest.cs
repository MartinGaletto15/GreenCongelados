using System.ComponentModel.DataAnnotations;

namespace Applications.dtos.Requests;

public record UpdateShippingCostRequest(
    [MaxLength(50)] string? Name,
    decimal? Cost
);
