using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record CreateShippingCostRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    string Name,
    [Required(ErrorMessage = "El costo es obligatorio.")] decimal Cost,
    [Required(ErrorMessage = "El estado activo es obligatorio.")] bool IsActive
);
