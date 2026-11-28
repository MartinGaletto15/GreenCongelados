using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record UpdateAddressRequest(
    [MaxLength(100, ErrorMessage = "La calle no puede superar los 100 caracteres.")]
    string? Street,
    [MaxLength(10, ErrorMessage = "El departamento no puede superar los 10 caracteres.")]
    string? Dpto,
    string? References
);
