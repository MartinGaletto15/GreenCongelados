namespace Application.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public record ResetPasswordRequest(
    [Required]
    string Token,

    [Required]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    string NewPassword
);