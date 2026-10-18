namespace Application.DTOs.Requests;
using System.ComponentModel.DataAnnotations;

public record ResetPasswordRequest(
    [Required] string Token,
    [Required] string NewPassword
);