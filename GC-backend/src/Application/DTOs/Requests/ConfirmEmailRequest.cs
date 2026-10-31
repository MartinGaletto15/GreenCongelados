using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record ConfirmEmailRequest(
    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(64, ErrorMessage = "El código no es válido.")]
    string Token
);
