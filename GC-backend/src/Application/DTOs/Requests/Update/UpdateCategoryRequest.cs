using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record UpdateCategoryRequest(
    [MaxLength(50)] string? Name,
    string? ImageUrl
);
