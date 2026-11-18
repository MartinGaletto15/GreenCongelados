namespace Application.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public record ResetPasswordRequest(
    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(64, ErrorMessage = "El código no es válido.")]
    string Token,
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    string NewPassword
);
