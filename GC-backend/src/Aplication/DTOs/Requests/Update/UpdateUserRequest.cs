namespace Applications.dtos.Requests;

public record UpdateUserRequest(
    [MaxLength(50)] string? Name,
    [MaxLength(50)] string? LastName,
    [MaxLength(100)] string? Email,
    [MinLength(6)] string? Password,
    [MaxLength(25)] string? Phone
);