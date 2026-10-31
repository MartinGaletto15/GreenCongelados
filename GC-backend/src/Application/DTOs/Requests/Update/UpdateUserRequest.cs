using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record UpdateUserRequest(
    [StringLength(50, MinimumLength = 2)] string? Name,
    [StringLength(50, MinimumLength = 2)] string? LastName,
    [StringLength(20, MinimumLength = 6)]
    [RegularExpression(@"^(?=.*\p{Lu})(?=.*\p{Nd})[\s\S]+$", ErrorMessage = "La contraseña debe incluir al menos una mayúscula y un número.")]
    string? Password,
    [MaxLength(25)] string? Phone
);