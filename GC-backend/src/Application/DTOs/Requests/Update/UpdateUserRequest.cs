using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

// La contraseña no se actualiza por este endpoint: el cambio solo se permite
// mediante el flujo verificado con código (PasswordResetService).
public record UpdateUserRequest(
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")] string? Name,
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres.")] string? LastName,
    [MaxLength(25, ErrorMessage = "El teléfono no puede superar los 25 caracteres.")] string? Phone
);