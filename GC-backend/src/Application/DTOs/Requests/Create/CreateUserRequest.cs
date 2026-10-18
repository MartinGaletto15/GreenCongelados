using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record CreateUserRequest(
    [Required] string Name,
    [Required] string LastName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);