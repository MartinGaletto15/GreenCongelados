namespace Applications.dtos;

public record OrderDTO(
    UserDTO? Courier,
    PromotionDTO? Promotion,
    decimal ShippingCost,
    string ShippingStreet,
    string ShippingDpto,
    string ShippingReference,
    DateTime OrderDate,
    decimal GlobalDiscount,
    decimal Subtotal,
    decimal Total,
    string OrderStatus
)
{
    public static OrderDTO Create(Domain.Entities.Order entity)
    {
        return new OrderDTO(
            entity.Courier != null ? UserDTO.Create(entity.Courier) : null,
            entity.Promotion != null ? PromotionDTO.Create(entity.Promotion) : null,
            entity.ShippingCost,
            entity.ShippingStreet,
            entity.ShippingDpto,
            entity.ShippingReference,
            entity.OrderDate,
            entity.GlobalDiscount,
            entity.Subtotal,
            entity.Total,
            entity.OrderStatus.ToString()
        );
    }

    public static List<OrderDTO> Create(IEnumerable<Domain.Entities.Order> entities)
    {
        return entities.Select(Create).ToList();
    }
}
