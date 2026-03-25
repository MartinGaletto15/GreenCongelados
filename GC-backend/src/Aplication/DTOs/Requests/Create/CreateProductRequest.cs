using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

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
    [EnumDataType(typeof(ProductStatus))] ProductStatus Status,
    List<int>? CategoryIds = null
);
