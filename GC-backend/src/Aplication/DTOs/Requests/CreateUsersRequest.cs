namespace Applications.dtos.Requests;

public record CreateUsersRequest(
    [Required] string Name,
    [Required] string LastName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);