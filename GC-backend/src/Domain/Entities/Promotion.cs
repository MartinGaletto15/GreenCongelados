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
        ValidateData(couponCode, discountValue, startDate, endDate);

        CouponCode = NormalizeCouponCode(couponCode);
        DiscountValue = discountValue;
        DiscountType = discountType;
        MinAmount = minAmount;
        StartDate = startDate;
        EndDate = endDate;
    }

    public bool IsActive(decimal amount)
    {
        var now = DateTime.Now;
        if (now < StartDate || now > EndDate) return false;
        if (MinAmount.HasValue && amount < MinAmount.Value) return false;
        return true;
    }

    public void UpdateDetails(string? couponCode, decimal? discountValue, DiscountType? discountType, decimal? minAmount, DateTime? startDate, DateTime? endDate)
    {
        if (couponCode != null)
        {
            if (string.IsNullOrWhiteSpace(couponCode)) throw new AppValidationException("El código de cupón no puede estar vacío.", "INVALID_COUPON");
            CouponCode = NormalizeCouponCode(couponCode);
        }

        if (discountValue.HasValue)
        {
            if (discountValue.Value < 0) throw new AppValidationException("El valor del descuento no puede ser negativo.", "INVALID_DISCOUNT");
            DiscountValue = discountValue.Value;
        }

        DiscountType = discountType ?? DiscountType;
        MinAmount = minAmount ?? MinAmount;
        StartDate = startDate ?? StartDate;
        EndDate = endDate ?? EndDate;

        if (StartDate > EndDate) throw new AppValidationException("La fecha de inicio no puede ser posterior a la fecha de fin.", "INVALID_DATES");
    }

    private void ValidateData(string couponCode, decimal discountValue, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(couponCode)) throw new AppValidationException("El código de cupón no puede estar vacío.", "INVALID_COUPON");
        if (discountValue < 0) throw new AppValidationException("El valor del descuento no puede ser negativo.", "INVALID_DISCOUNT");
        if (startDate > endDate) throw new AppValidationException("La fecha de inicio no puede ser posterior a la fecha de fin.", "INVALID_DATES");
    }

    public static string NormalizeCouponCode(string couponCode) => couponCode.Trim().ToUpperInvariant();
}
