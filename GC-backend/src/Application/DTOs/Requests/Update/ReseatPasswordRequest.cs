namespace Application.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public record ResetPasswordRequest(
    [Required(ErrorMessage = "El código es obligatorio.")]
    string Token,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    string NewPassword
);
