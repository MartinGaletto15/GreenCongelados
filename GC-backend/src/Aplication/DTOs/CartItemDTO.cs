namespace Applications.dtos;

public record CartItemDTO(
    int Id,
    int ProductId,
    int Quantity
)
{
    public static CartItemDTO Create(Domain.Entities.CartItem entity)
    {
        var dto = new CartItemDTO(
            entity.IdCartItem,
            entity.IdProduct,
            entity.Quantity
        );
        return dto;
    }

    public static List<CartItemDTO> Create(IEnumerable<Domain.Entities.CartItem> entities)
    {
        var listDto = new List<CartItemDTO>();
        foreach (var entity in entities)
        {
            listDto.Add(Create(entity));
        }
        return listDto;
    }
}