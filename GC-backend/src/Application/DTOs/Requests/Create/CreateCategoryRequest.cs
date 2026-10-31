using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record CreateCategoryRequest(
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    string Name,
    [Required(ErrorMessage = "La imagen de la categoría es obligatoria.")] string ImageUrl
);
