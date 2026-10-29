using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record ResendEmailConfirmationRequest(
    [Required(ErrorMessage = "El email es obligatorio."), EmailAddress(ErrorMessage = "Ingresa un email válido.")] string Email
);
