using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record CompleteRegistrationRequest(
    [Required(ErrorMessage = "El código es obligatorio.")] string Token,
    [Required(ErrorMessage = "El nombre es obligatorio.")] string Name,
    [Required(ErrorMessage = "El apellido es obligatorio.")] string LastName,
    [Required(ErrorMessage = "La contraseña es obligatoria."), MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")] string Password
);
