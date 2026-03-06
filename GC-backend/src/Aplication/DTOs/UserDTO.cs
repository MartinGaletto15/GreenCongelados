namespace Applications.dtos;

public record UserDTO(
    string Name,
    string LastName,
    string Email,
    string Phone
)
{
    public static UserDTO Create(Domain.Entities.User entity)
    {
        var dto = new UserDTO(
            entity.Name,
            entity.LastName,
            entity.Email,
            entity.Phone
        );
        return dto;
    }

    public static List<UserDTO> Create(IEnumerable<Domain.Entities.User> entities)
    {
        var listDto = new List<UserDTO>();
        foreach (var entity in entities)
        {
            listDto.Add(Create(entity));
        }
        return listDto;
    }
}