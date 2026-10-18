namespace Application.DTOs.Requests;
using System.ComponentModel.DataAnnotations;

public record ForgotPasswordRequest(
    [Required] [EmailAddress] string Email
);