using Application.DTOs;
using Application.DTOs.Requests;
using Domain.Entities;
using Domain.Entities.Enums;

namespace Application.Interfaces.UserServices;

public interface IUserWriteService
{
    Task<UserDTO> CreateUserAsync(CreateUserRequest user, CancellationToken cancellationToken = default);
    Task<UserDTO> UpdateUserAsync(int id, UpdateUserRequest user);
    Task<UserDTO> UpdateUserRoleAsync(int id, Role role, Role performerRole);
    Task<UserDTO> DeleteUserAsync(int id);
    Task<string> LoginAsync(LoginRequest request);
}