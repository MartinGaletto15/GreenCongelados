using System.ComponentModel.DataAnnotations;

namespace Applications.dtos.Requests;

public record CreateCategoryRequest(
    [Required] [MaxLength(50)] string Name,
    [Required] string ImageUrl
);
