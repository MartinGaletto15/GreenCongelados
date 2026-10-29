namespace Application.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public record ResetPasswordRequest(
    [Required(ErrorMessage = "El código es obligatorio.")]
    string Token,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 20 caracteres.")]
    [RegularExpression(@"^(?=.*\p{Lu})(?=.*\p{Nd})[\s\S]+$", ErrorMessage = "La contraseña debe incluir al menos una mayúscula y un número.")]
    string NewPassword
);
