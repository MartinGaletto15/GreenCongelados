using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Application.DTOs.Requests;

public record CreatePromotionRequest(
    [Required] [MinLength(10)] [MaxLength(15)] string CouponCode,
    [Required] decimal DiscountValue,
    [Required] DiscountType DiscountType,
    decimal? MinAmount,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate
);
