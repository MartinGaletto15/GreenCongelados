using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record CompleteRegistrationRequest(
    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(64, ErrorMessage = "El código no es válido.")]
    string Token,

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")]
    string Name,

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres.")]
    string LastName,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 20 caracteres.")]
    [RegularExpression(@"^(?=.*\p{Lu})(?=.*\p{Nd})[\s\S]+$", ErrorMessage = "La contraseña debe incluir al menos una mayúscula y un número.")]
    string Password
);
