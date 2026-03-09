using System.ComponentModel.DataAnnotations;
using Domain.Enums.Entities;

namespace Applications.dtos.Requests;

public record UpdatePromotionRequest(
    [MinLength(10)] [MaxLength(15)] string? CouponCode,
    decimal? DiscountValue,
    DiscountType? DiscountType,
    decimal? MinAmount,
    DateTime? StartDate,
    DateTime? EndDate
);
