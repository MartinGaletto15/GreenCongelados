using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record UpdateShippingCostRequest(
    [MaxLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    string? Name = null,
    decimal? Cost = null,
    bool? IsActive = null
);
