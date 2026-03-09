using System.ComponentModel.DataAnnotations;

namespace Applications.dtos.Requests;

public record UpdateProductCategoryRequest(
    int? IdProduct,
    int? IdCategory
);
