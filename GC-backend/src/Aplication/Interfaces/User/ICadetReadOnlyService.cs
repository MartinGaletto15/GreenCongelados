using Applications.dtos;

namespace Aplication.Interfaces.User;

public interface ICadetReadOnlyService
{
    Task<IEnumerable<UserDTO>> GetAllCadetsAsync();
    Task<UserDTO> GetCadetByIdAsync(int id);
}