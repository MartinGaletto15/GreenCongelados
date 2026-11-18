using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record CompleteRegistrationRequest(
    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(64, ErrorMessage = "El código no es válido.")]
    string Token,

    // El largo y la normalización viven en User.ValidateName (única fuente de verdad).
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    string Name,

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    string LastName,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    string Password
);
