using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record UpdateOrderItemRequest(
    int? Quantity,
    decimal? UnitPrice
);