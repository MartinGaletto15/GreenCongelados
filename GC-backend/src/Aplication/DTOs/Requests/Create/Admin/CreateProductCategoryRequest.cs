using System.ComponentModel.DataAnnotations;

namespace Applications.dtos.Requests;

public record CreateProductCategoryRequest(
    [Required] int IdProduct,
    [Required] int IdCategory
);
