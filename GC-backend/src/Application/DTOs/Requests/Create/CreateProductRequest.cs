using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Application.DTOs.Requests;

public record CreateProductRequest(
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    string Name,
    [MaxLength(200, ErrorMessage = "La URL de la imagen no puede superar los 200 caracteres.")]
    string? UrlImagePrimary,
    [Required(ErrorMessage = "La descripción corta es obligatoria.")]
    [MaxLength(255, ErrorMessage = "La descripción corta no puede superar los 255 caracteres.")]
    string DescriptionShort,
    string? DescriptionLong,
    [Required(ErrorMessage = "El precio es obligatorio.")] decimal Price,
    [Required(ErrorMessage = "El stock es obligatorio.")] int CurrentStock,
    decimal? Weight,
    decimal? PreparationTime,
    [EnumDataType(typeof(ProductStatus), ErrorMessage = "El estado del producto no es válido.")] ProductStatus Status
);
