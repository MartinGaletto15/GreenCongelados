namespace Applications.dtos;

public record OrderDTO(
    int IdOrder,
    UserDTO? Courier,
    PromotionDTO? Promotion,
    decimal ShippingCost,
    string ShippingStreet,
    string? ShippingDpto,
    string? ShippingReference,
    DateTime OrderDate,
    decimal GlobalDiscount,
    decimal Subtotal,
    decimal Total,
    string OrderStatus,
    List<OrderItemDTO> OrderItems
)
{
    public static OrderDTO Create(Domain.Entities.Order entity)
    {
        return new OrderDTO(
            entity.IdOrder,
            entity.Courier != null ? UserDTO.Create(entity.Courier) : null,
            entity.Promotion != null ? 
                (entity.Promotion.DiscountType == Domain.Entities.Enums.DiscountType.FreeShipping 
                    ? PromotionDTO.Create(entity.Promotion) with { DiscountValue = entity.GlobalDiscount } 
                    : PromotionDTO.Create(entity.Promotion)) 
                : null,
            entity.ShippingCost,
            entity.ShippingStreet,
            entity.ShippingDpto,
            entity.ShippingReference,
            entity.OrderDate,
            entity.GlobalDiscount,
            entity.Subtotal,
            entity.Total,
            entity.OrderStatus.ToString(),
            entity.OrderItems != null ? OrderItemDTO.Create(entity.OrderItems) : new List<OrderItemDTO>()
        );
    }

    public static List<OrderDTO> Create(IEnumerable<Domain.Entities.Order> entities)
    {
        return entities.Select(Create).ToList();
    }
}
