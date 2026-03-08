using System.ComponentModel.DataAnnotations;

namespace Applications.dtos.Requests;

public record CreateProductRequest(
    [Required] [MaxLength(100)] string Name,
    [MaxLength(200)] string? UrlImagePrimary,
    [Required] [MaxLength(255)] string DescriptionShort,
    string? DescriptionLong,
    [Required] decimal Price,
    [Required] int CurrentStock,
    decimal? Weight,
    decimal? PreparationTime,
    [Required] [MaxLength(25)] string ProductStatus,
    List<int>? CategoryIds
);
