using Application.DTOs;

namespace Application.Interfaces.User;

public interface ICadetReadOnlyService
{
    Task<IEnumerable<UserDTO>> GetAllCadetsAsync();
    Task<UserDTO> GetCadetByIdAsync(int id);
}