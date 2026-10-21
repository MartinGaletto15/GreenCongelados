using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record ConfirmEmailRequest(
    [Required] string Token
);