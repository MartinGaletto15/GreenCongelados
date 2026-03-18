using Applications.dtos;
using Applications.dtos.Requests;
using Domain.Entities;
using Domain.Entities.Enums;

namespace Aplication.Interfaces.UserServices;

public interface IUserWriteService
{
    Task<UserDTO> CreateUserAsync(CreateUserRequest user);
    Task<UserDTO> UpdateUserAsync(int id, UpdateUserRequest user);
    Task<UserDTO> UpdateUserRoleAsync(int id, Role role, Role performerRole);
    Task<UserDTO> DeleteUserAsync(int id);
    Task<string> LoginAsync(LoginRequest request);
}