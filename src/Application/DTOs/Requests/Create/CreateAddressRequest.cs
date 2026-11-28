using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Application.DTOs.Requests;

public record CreateAddressRequest(
    [Required(ErrorMessage = "La calle es obligatoria.")]
    [MaxLength(100, ErrorMessage = "La calle no puede superar los 100 caracteres.")]
    string Street,
    [MaxLength(10, ErrorMessage = "El departamento no puede superar los 10 caracteres.")]
    string? Dpto,
    string? References
);