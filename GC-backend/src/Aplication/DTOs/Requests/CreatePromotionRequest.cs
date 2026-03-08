using System.ComponentModel.DataAnnotations;
using Domain.Enums.Entities;

namespace Applications.dtos.Requests;

public record CreatePromotionRequest(
    [Required] [MinLength(10)] [MaxLength(15)] string CouponCode,
    [Required] decimal DiscountValue,
    [Required] DiscountType DiscountType,
    decimal? MinAmount,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate
);
