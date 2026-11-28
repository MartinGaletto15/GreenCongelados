using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record CreateUserRequest(
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un email válido.")]
    [MaxLength(100, ErrorMessage = "El email no puede superar los 100 caracteres.")]
    string Email
);
