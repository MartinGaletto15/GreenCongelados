namespace Application.DTOs.Requests;
using System.ComponentModel.DataAnnotations;

public record ForgotPasswordRequest(
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un email válido.")]
    [MaxLength(100, ErrorMessage = "El email no puede superar los 100 caracteres.")]
    string Email
);
