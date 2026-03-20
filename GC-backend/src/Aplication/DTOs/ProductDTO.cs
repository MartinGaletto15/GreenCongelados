namespace Applications.dtos;

public record ProductDTO(
    int IdProduct,
    string Name,
    string? UrlImagePrimary,
    string DescriptionShort,
    string? DescriptionLong,
    decimal Price,
    int CurrentStock,
    decimal? Weight,
    decimal? PreparationTime,
    string ProductStatus,
    bool IsActive
)
{
    public static ProductDTO Create(Domain.Entities.Product entity)
    {
        var dto = new ProductDTO(
            entity.IdProduct,
            entity.Name,
            entity.UrlImagePrimary,
            entity.DescriptionShort,
            entity.DescriptionLong,
            entity.Price,
            entity.CurrentStock,
            entity.Weight,
            entity.PreparationTime,
            entity.Status.ToString(),
            entity.IsActive
        );
        return dto;
    }

    public static List<ProductDTO> Create(IEnumerable<Domain.Entities.Product> entities)
    {
        var listDto = new List<ProductDTO>();
        foreach (var entity in entities)
        {
            listDto.Add(Create(entity));
        }
        return listDto;
    }
}