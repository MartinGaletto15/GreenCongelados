using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record UpdateCategoryRequest(
    [MaxLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    string? Name,
    string? ImageUrl
);
