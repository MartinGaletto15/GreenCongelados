namespace Applications.dtos;

public record OrderItemDTO(
    ProductDTO Product,
    int Quantity,
    decimal UnitPrice
)
{
    public static OrderItemDTO Create(Domain.Entities.OrderItem entity)
    {
        var dto = new OrderItemDTO(
            ProductDTO.Create(entity.Product),
            entity.Quantity,
            entity.UnitPrice
        );
        return dto;
    }

    public static List<OrderItemDTO> Create(IEnumerable<Domain.Entities.OrderItem> entities)
    {
        var listDto = new List<OrderItemDTO>();
        foreach (var entity in entities)
        {
            listDto.Add(Create(entity));
        }
        return listDto;
    }
}
