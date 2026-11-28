using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Application.DTOs.Requests;

public record UpdatePromotionRequest(
    [MinLength(10, ErrorMessage = "El código de cupón debe tener al menos 10 caracteres.")]
    [MaxLength(15, ErrorMessage = "El código de cupón no puede superar los 15 caracteres.")]
    string? CouponCode,
    decimal? DiscountValue,
    DiscountType? DiscountType,
    decimal? MinAmount,
    DateTime? StartDate,
    DateTime? EndDate
);
