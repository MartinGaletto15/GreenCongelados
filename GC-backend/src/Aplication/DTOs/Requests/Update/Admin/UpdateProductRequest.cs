using System.ComponentModel.DataAnnotations;

namespace Applications.dtos.Requests;

public record UpdateProductRequest(
    [MaxLength(100)] string? Name,
    [MaxLength(200)] string? UrlImagePrimary,
    [MaxLength(255)] string? DescriptionShort,
    string? DescriptionLong,
    decimal? Price,
    int? CurrentStock,
    decimal? Weight,
    decimal? PreparationTime,
    [MaxLength(25)] string? ProductStatus,
    List<int>? CategoryIds
);
