namespace Applications.dtos;

public record AddressDTO(
    string Street,
    string City,
    string ZipCode,
    string Dpto,
    string References
)
{
    public static AddressDTO Create(Domain.Entities.Address entity)
    {
        var dto = new AddressDTO(
            entity.Street,
            entity.City.ToString(),
            entity.ZipCode,
            entity.Dpto,
            entity.References
        );
        return dto;
    }

    public static List<AddressDTO> Create(IEnumerable<Domain.Entities.Address> entities)
    {
        var listDto = new List<AddressDTO>();
        foreach (var entity in entities)
        {
            listDto.Add(Create(entity));
        }
        return listDto;
    }
}