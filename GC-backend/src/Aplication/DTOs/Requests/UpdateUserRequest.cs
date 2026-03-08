namespace Applications.dtos.Requests;

public record UpdateUserRequest(
    [Required] string? Name,
    [Required] string? LastName,
    [Required] string? Email,
    [Required] string? Password,
    [Required] string? Phone
);