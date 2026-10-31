using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record UpdateUserRequest(
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")] string? Name,
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres.")] string? LastName,
    [StringLength(20, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 20 caracteres.")]
    [RegularExpression(@"^(?=.*\p{Lu})(?=.*\p{Nd})[\s\S]+$", ErrorMessage = "La contraseña debe incluir al menos una mayúscula y un número.")]
    string? Password,
    [MaxLength(25, ErrorMessage = "El teléfono no puede superar los 25 caracteres.")] string? Phone
);