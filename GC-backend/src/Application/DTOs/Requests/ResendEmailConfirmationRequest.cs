using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record ResendEmailConfirmationRequest(
    [Required, EmailAddress] string Email
);