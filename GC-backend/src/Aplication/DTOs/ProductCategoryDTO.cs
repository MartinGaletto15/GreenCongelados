namespace Applications.dtos;

public record ProductCategoryDTO(
    ProductDTO Product,
    CategoryDTO Category
)
{
    public static ProductCategoryDTO Create(Domain.Entities.ProductCategory entity)
    {
        var dto = new ProductCategoryDTO(
            ProductDTO.Create(entity.Product),
            CategoryDTO.Create(entity.Category)
        );
        return dto;
    }

    public static List<ProductCategoryDTO> Create(IEnumerable<Domain.Entities.ProductCategory> entities)
    {
        var listDto = new List<ProductCategoryDTO>();
        foreach (var entity in entities)
        {
            listDto.Add(Create(entity));
        }
        return listDto;
    }
}