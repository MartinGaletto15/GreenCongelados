using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record UpdateUserRequest(
    [MaxLength(50)] string? Name,
    [MaxLength(50)] string? LastName,
    [MinLength(6)] string? Password,
    [MaxLength(25)] string? Phone
);