using Applications.dtos;
using Applications.dtos.Requests;
using Domain.Entities;

namespace Aplication.Interfaces.UserServices;

public interface IUserWriteService
{
    Task<User> CreateUserAsync(CreateUserRequest user);
    Task<User> UpdateUserAsync(UpdateUserRequest user);
    Task<User> DeleteUserAsync(int id);
}