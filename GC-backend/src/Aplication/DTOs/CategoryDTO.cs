namespace Applications.dtos;

public record CategoryDTO(
    string Name,
    string? UrlImage
)
{
    public static CategoryDTO Create(Domain.Entities.Category entity)
    {
        var dto = new CategoryDTO(
            entity.Name,
            entity.UrlImage
        );
        return dto;
    }

    public static List<CategoryDTO> Create(IEnumerable<Domain.Entities.Category> entities)
    {
        var listDto = new List<CategoryDTO>();
        foreach (var entity in entities)
        {
            listDto.Add(Create(entity));
        }
        return listDto;
    }
}