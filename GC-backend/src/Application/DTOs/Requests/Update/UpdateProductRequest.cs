using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Application.DTOs.Requests;

public record UpdateProductRequest(
    [MaxLength(100)] string? Name,
    [MaxLength(200)] string? UrlImagePrimary,
    [MaxLength(255)] string? DescriptionShort,
    string? DescriptionLong,
    decimal? Price,
    int? CurrentStock,
    decimal? Weight,
    decimal? PreparationTime,
    [EnumDataType(typeof(ProductStatus))] ProductStatus? Status
);
