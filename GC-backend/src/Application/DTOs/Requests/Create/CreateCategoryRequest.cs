using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record CreateCategoryRequest(
    [Required] [MaxLength(50)] string Name,
    [Required] string ImageUrl
);
