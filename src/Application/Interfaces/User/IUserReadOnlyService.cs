using Application.DTOs;

namespace Application.Interfaces.UserServices;

public interface IUserReadOnlyService
{
    Task<UserDTO> GetUserByIdAsync(int id);
    Task<IEnumerable<UserDTO>> GetAllUsersAsync();
}