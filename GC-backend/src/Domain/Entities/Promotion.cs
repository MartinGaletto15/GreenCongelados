using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class Promotion
{
    [Key]
    public int IdPromotion { get; private set; }

    [Required]
    [MinLength(10)]
    [MaxLength(15)]
    public string CouponCode { get; private set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal DiscountValue { get; private set; }

    [Required]
    public DiscountType DiscountType { get; private set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MinAmount { get; private set; }

    [Required]
    public DateTime StartDate { get; private set; }

    [Required]
    public DateTime EndDate { get; private set; }

    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    protected Promotion() {
        CouponCode = string.Empty;
    }

    public Promotion(string couponCode, decimal discountValue, DiscountType discountType, decimal? minAmount, DateTime startDate, DateTime endDate)
    {
        ValidateData(couponCode, discountValue, discountType, minAmount, startDate, endDate);

        CouponCode = NormalizeCouponCode(couponCode);
        DiscountValue = discountValue;
        DiscountType = discountType;
        MinAmount = minAmount;
        StartDate = startDate;
        EndDate = endDate;
    }

    public decimal CalculateDiscount(decimal subtotal, decimal shippingCost)
    {
        var now = DateTime.Now;
        if (now < StartDate || now > EndDate)
            throw new AppValidationException("El cupón de promoción expiró o todavía no es válido.", "PROMOTION_EXPIRED");

        if (MinAmount.HasValue && subtotal < MinAmount.Value)
            throw new AppValidationException($"El monto mínimo para aplicar la promoción es {MinAmount.Value}.", "PROMOTION_MIN_AMOUNT");

        var discount = DiscountType switch
        {
            DiscountType.Percentage => Math.Round(subtotal * (DiscountValue / 100m), 2, MidpointRounding.AwayFromZero),
            DiscountType.FixedAmount => DiscountValue,
            DiscountType.FreeShipping => shippingCost,
            _ => throw new AppValidationException("El tipo de descuento no es válido.", "INVALID_DISCOUNT_TYPE")
        };

        return Math.Min(discount, subtotal + shippingCost);
    }

    public void UpdateDetails(string? couponCode, decimal? discountValue, DiscountType? discountType, decimal? minAmount, DateTime? startDate, DateTime? endDate)
    {
        var newCoupon = couponCode != null ? NormalizeCouponCode(couponCode) : CouponCode;
        var newValue = discountValue ?? DiscountValue;
        var newType = discountType ?? DiscountType;
        var newMin = minAmount ?? MinAmount;
        var newStart = startDate ?? StartDate;
        var newEnd = endDate ?? EndDate;

        ValidateData(newCoupon, newValue, newType, newMin, newStart, newEnd);

        CouponCode = newCoupon;
        DiscountValue = newValue;
        DiscountType = newType;
        MinAmount = newMin;
        StartDate = newStart;
        EndDate = newEnd;
    }

    private static void ValidateData(string couponCode, decimal discountValue, DiscountType discountType, decimal? minAmount, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(couponCode)) throw new AppValidationException("El código de cupón no puede estar vacío.", "INVALID_COUPON");

        var normalizedCouponCode = NormalizeCouponCode(couponCode);
        if (normalizedCouponCode.Length < 10 || normalizedCouponCode.Length > 15)
            throw new AppValidationException("El código de cupón debe tener entre 10 y 15 caracteres.", "INVALID_COUPON");

        if (!Enum.IsDefined(discountType)) throw new AppValidationException("El tipo de descuento no es válido.", "INVALID_DISCOUNT_TYPE");
        if (discountValue < 0) throw new AppValidationException("El valor del descuento no puede ser negativo.", "INVALID_DISCOUNT");
        if (discountType == DiscountType.Percentage && discountValue > 100) throw new AppValidationException("El porcentaje de descuento no puede superar 100.", "INVALID_DISCOUNT");
        if (minAmount.HasValue && minAmount.Value < 0) throw new AppValidationException("El monto mínimo no puede ser negativo.", "INVALID_MIN_AMOUNT");
        if (startDate > endDate) throw new AppValidationException("La fecha de inicio no puede ser posterior a la fecha de fin.", "INVALID_DATES");
    }

    public static string NormalizeCouponCode(string couponCode) => couponCode.Trim().ToUpperInvariant();
}
