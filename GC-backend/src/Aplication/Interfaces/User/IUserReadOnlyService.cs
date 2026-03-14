using Applications.dtos;

namespace Aplication.Interfaces.UserServices;

public interface IUserReadOnlyService
{
    Task<UserDTO> GetUserByIdAsync(int id);
    Task<IEnumerable<UserDTO>> GetAllUsersAsync();
}