using Applications.dtos;
using Applications.dtos.Requests;
using Aplication.Interfaces.UserServices;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Aplication.Services;

public class UserService : IUserWriteService, IUserReadOnlyService
{
    private readonly IGenericRepository<User> _userRepository;

    public UserService(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        // Ejemplo de validación de negocio usando tu Exception personalizada
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new AppValidationException("El email es obligatorio para crear un usuario", "USER_EMAIL_REQUIRED");

        var newUser = new User
        {
            Name = request.Name,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password
        };

        await _userRepository.AddAsync(newUser);
        return newUser;
    }

    public async Task<User> UpdateUserAsync(UpdateUserRequest request)
    {
        // Aca va la busqueda del usuario previa a la modificacion
        
        if (string.IsNullOrEmpty(request.Email))
            throw new AppValidationException("No se puede actualizar un usuario sin un email de referencia", "USER_UPDATE_EMAIL_MISSING");

        var existingUser = new User // Esto es un ejemplo, normalmente harías un GetById primero
        {
            Name = request.Name,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password,
            Phone = request.Phone
        };

        await _userRepository.UpdateAsync(existingUser);
        return existingUser;
    }

    public async Task<User> DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        // Uso de AppValidationException si el usuario no existe
        if (user == null)
            throw new AppValidationException($"No se puede eliminar: El usuario con ID {id} no existe.", "USER_NOT_FOUND");

        await _userRepository.DeleteAsync(user);
        return user;
    }

    public async Task<UserDTO> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        if (user == null)
            throw new AppValidationException($"Usuario con ID {id} no encontrado", "USER_NOT_FOUND");

        return UserDTO.Create(user);
    }

    public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        
        if (users == null || !users.Any())
            throw new AppValidationException("No hay usuarios registrados en el sistema", "USER_LIST_EMPTY");

        return UserDTO.Create(users);
    }
}