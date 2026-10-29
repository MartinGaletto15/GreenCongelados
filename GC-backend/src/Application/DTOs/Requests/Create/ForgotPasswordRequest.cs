namespace Application.DTOs.Requests;
using System.ComponentModel.DataAnnotations;

public record ForgotPasswordRequest(
    [Required(ErrorMessage = "El email es obligatorio."), EmailAddress(ErrorMessage = "Ingresa un email válido.")] string Email
);
