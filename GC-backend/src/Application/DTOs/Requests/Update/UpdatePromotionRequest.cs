using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Application.DTOs.Requests;

public record UpdatePromotionRequest(
    [MinLength(10)] [MaxLength(15)] string? CouponCode,
    decimal? DiscountValue,
    DiscountType? DiscountType,
    decimal? MinAmount,
    DateTime? StartDate,
    DateTime? EndDate
);
