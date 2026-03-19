using System.Security.Claims;
using Applications.dtos;
using Applications.dtos.Requests;
using Aplication.Helpers;
using Aplication.Interfaces.Security;
using Aplication.Interfaces.UserServices;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Aplication.Interfaces.CartItem;


namespace Aplication.Services;

public class UserService : IUserWriteService, IUserReadOnlyService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly ICartItemWriteService _cartItemWriteService;

    public UserService(
        IUserRepository userRepository, 
        IConfiguration configuration, 
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        ICartItemWriteService cartItemWriteService)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _cartItemWriteService = cartItemWriteService;
    }

    public async Task<string> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !_passwordHasher.Verify(request.Password, user.Password))
            throw new AppValidationException("Credenciales inválidas");

        return _jwtProvider.GenerateToken(user);
    }

    public async Task<UserDTO> CreateUserAsync(CreateUserRequest request)
    {
        if (!Validations.ValidateEmail(request.Email))
            throw new AppValidationException("El email provisto no es válido.", "USER_EMAIL_INVALID");

        if (!Validations.ValidatePassword(request.Password, 6, 20, true, true))
            throw new AppValidationException("La contraseña debe tener entre 6 y 20 caracteres, e incluir al menos una mayúscula y un número.", "USER_PASSWORD_INVALID");

        if (!Validations.ValidateString(request.Name, 2, 50))
            throw new AppValidationException("El nombre es obligatorio y debe tener entre 2 y 50 caracteres.", "USER_NAME_INVALID");

        if (!Validations.ValidateString(request.LastName, 2, 50))
            throw new AppValidationException("El apellido es obligatorio y debe tener entre 2 y 50 caracteres.", "USER_LASTNAME_INVALID");

        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new AppValidationException("El email ya existe en la base de datos.", "USER_EMAIL_EXISTS");

        string passwordHash = _passwordHasher.Hash(request.Password);

        var newUser = new User
        {
            Name = request.Name,
            LastName = request.LastName,
            Email = request.Email,
            Password = passwordHash
        };

        await _userRepository.AddAsync(newUser);
        // Aca se crea el carrito del usuario
        await _cartItemWriteService.CreateCartAsync(newUser.IdUser);
        return UserDTO.Create(newUser);
    }

    public async Task<UserDTO> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        if (!Validations.ValidateEmail(request.Email))
            throw new AppValidationException("El email de referencia provisto no es válido.", "USER_EMAIL_INVALID");

        if (!string.IsNullOrEmpty(request.Password) && !Validations.ValidatePassword(request.Password, 6, 20, true, true))
            throw new AppValidationException("La contraseña debe tener entre 6 y 20 caracteres, e incluir al menos una mayúscula y un número.", "USER_PASSWORD_INVALID");

        if (!string.IsNullOrEmpty(request.Name) && !Validations.ValidateString(request.Name, 2, 50))
            throw new AppValidationException("El nombre debe tener entre 2 y 50 caracteres.", "USER_NAME_INVALID");

        if (!string.IsNullOrEmpty(request.LastName) && !Validations.ValidateString(request.LastName, 2, 50))
            throw new AppValidationException("El apellido debe tener entre 2 y 50 caracteres.", "USER_LASTNAME_INVALID");

        // Busqueda del usuario previa a la modificacion
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new AppValidationException("El usuario no existe.", "USER_NOT_FOUND");

        // Si el usuario intenta cambiar su mail, verificar que el nuevo no esté en uso por otro
        if (request.Email != null && user.Email != request.Email)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new AppValidationException("El email ya existe en la base de datos.", "USER_EMAIL_EXISTS");
        }

        string passwordHash = string.Empty;
        if (!string.IsNullOrEmpty(request.Password))
        {
            passwordHash = _passwordHasher.Hash(request.Password);
        }
        
        user.Name = request.Name ?? user.Name;
        user.LastName = request.LastName ?? user.LastName;
        user.Password = string.IsNullOrEmpty(passwordHash) ? user.Password : passwordHash;
        user.Phone = request.Phone ?? user.Phone;

        await _userRepository.UpdateAsync(user);
        return UserDTO.Create(user);
    }

    public async Task<UserDTO> UpdateUserRoleAsync(int id, Role role, Role performerRole)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        if (user == null)
            throw new AppValidationException("El usuario no existe.", "USER_NOT_FOUND");

        // El ejecutor debe tener una jerarquía superior (valor numérico menor) al rol actual del usuario
        if (performerRole >= user.Role)
            throw new AppValidationException("No tienes permisos para modificar el rol de este usuario.", "USER_ROLE_INSUFFICIENT_PERMISSIONS");

        // El ejecutor solo puede asignar roles de jerarquía inferior a la suya
        if (performerRole >= role)
            throw new AppValidationException("No tienes permisos para asignar este rol.", "USER_ROLE_INVALID_ASSIGNMENT");

        user.Role = role;
        await _userRepository.UpdateAsync(user);
        return UserDTO.Create(user);
    }

    public async Task<UserDTO> DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        // Uso de AppValidationException si el usuario no existe
        if (user == null)
            throw new AppValidationException($"No se puede eliminar: El usuario con ID {id} no existe.", "USER_NOT_FOUND");

        await _userRepository.DeleteAsync(user);
        return UserDTO.Create(user);
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