namespace Applications.dtos;

public record ShippingCostDTO(
    int Id,
    string Name,
    decimal Cost,
    bool IsActive
)
{
    public static ShippingCostDTO Create(Domain.Entities.ShippingCost entity)
    {
        var dto = new ShippingCostDTO(
            entity.IdShippingCost,
            entity.Name,
            entity.Cost,
            entity.IsActive
        );
        return dto;
    }

    public static List<ShippingCostDTO> Create(IEnumerable<Domain.Entities.ShippingCost> entities)
    {
        var listDto = new List<ShippingCostDTO>();
        foreach (var entity in entities)
        {
            listDto.Add(Create(entity));
        }
        return listDto;
    }
}
