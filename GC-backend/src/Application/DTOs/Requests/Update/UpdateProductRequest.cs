using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Application.DTOs.Requests;

public record UpdateProductRequest(
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    string? Name,
    [MaxLength(200, ErrorMessage = "La URL de la imagen no puede superar los 200 caracteres.")]
    string? UrlImagePrimary,
    [MaxLength(255, ErrorMessage = "La descripción corta no puede superar los 255 caracteres.")]
    string? DescriptionShort,
    string? DescriptionLong,
    decimal? Price,
    int? CurrentStock,
    decimal? Weight,
    decimal? PreparationTime,
    [EnumDataType(typeof(ProductStatus), ErrorMessage = "El estado del producto no es válido.")] ProductStatus? Status
);
