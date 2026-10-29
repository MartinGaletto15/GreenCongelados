using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record ConfirmEmailRequest(
    [Required(ErrorMessage = "El código es obligatorio.")] string Token
);
