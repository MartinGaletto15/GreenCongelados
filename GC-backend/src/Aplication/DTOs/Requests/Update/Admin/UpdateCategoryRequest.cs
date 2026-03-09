using System.ComponentModel.DataAnnotations;

namespace Applications.dtos.Requests;

public record UpdateCategoryRequest(
    [MaxLength(50)] string? Name,
    string? ImageUrl
);
