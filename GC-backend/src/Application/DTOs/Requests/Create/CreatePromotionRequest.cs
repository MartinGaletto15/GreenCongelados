using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Application.DTOs.Requests;

public record CreatePromotionRequest(
    [Required(ErrorMessage = "El código de cupón es obligatorio.")]
    [MinLength(10, ErrorMessage = "El código de cupón debe tener al menos 10 caracteres.")]
    [MaxLength(15, ErrorMessage = "El código de cupón no puede superar los 15 caracteres.")]
    string CouponCode,
    [Required(ErrorMessage = "El valor del descuento es obligatorio.")] decimal DiscountValue,
    [Required(ErrorMessage = "El tipo de descuento es obligatorio.")] DiscountType DiscountType,
    decimal? MinAmount,
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")] DateTime StartDate,
    [Required(ErrorMessage = "La fecha de fin es obligatoria.")] DateTime EndDate
);
