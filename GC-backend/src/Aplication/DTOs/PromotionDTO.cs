namespace Applications.dtos;

public record PromotionDTO(
    string CouponCode,
    decimal DiscountValue,
    string DiscountType,
    decimal? MinAmount,
    DateTime StartDate,
    DateTime EndDate
)
{
    public static PromotionDTO Create(Domain.Entities.Promotion entity)
    {
        var dto = new PromotionDTO(
            entity.CouponCode,
            entity.DiscountValue,
            entity.DiscountType,
            entity.MinAmount,
            entity.StartDate,
            entity.EndDate
        );
        return dto;
    }

    public static List<PromotionDTO> Create(IEnumerable<Domain.Entities.Promotion> entities)
    {
        var listDto = new List<PromotionDTO>();
        foreach (var entity in entities)
        {
            listDto.Add(Create(entity));
        }
        return listDto;
    }
}
