using System.Security.Claims;
using Applications.dtos;
using Applications.dtos.Requests;
using Aplication.Validators;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;

    public UserService(
        IUserRepository userRepository, 
        IConfiguration configuration, 
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        ICartItemWriteService cartItemWriteService,
        IUnitOfWork unitOfWork,
        IOrderRepository orderRepository)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _cartItemWriteService = cartItemWriteService;
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
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
        await UserValidator.ValidateCreateAsync(request, _userRepository);

        string passwordHash = _passwordHasher.Hash(request.Password);

        var newUser = new User(
            request.Name,
            request.LastName,
            request.Email,
            passwordHash
        );

        await _userRepository.AddAsync(newUser);
        await _unitOfWork.SaveChangesAsync();
        // Aca se crea el carrito del usuario
        await _cartItemWriteService.CreateCartAsync(newUser.IdUser);
        return UserDTO.Create(newUser);
    }

    public async Task<UserDTO> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new AppValidationException("El usuario no existe.", "USER_NOT_FOUND");

        await UserValidator.ValidateUpdateAsync(id, request, _userRepository, user);

        if (!string.IsNullOrEmpty(request.Password))
        {
            string passwordHash = _passwordHasher.Hash(request.Password);
            user.UpdatePassword(passwordHash);
        }
        
        user.UpdateDetails(request.Name, request.LastName, request.Phone);

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return UserDTO.Create(user);
    }

    public async Task<UserDTO> UpdateUserRoleAsync(int id, Role role, Role performerRole)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        if (user == null)
            throw new AppValidationException("El usuario no existe.", "USER_NOT_FOUND");

        UserValidator.ValidateRoleUpdate(role, user.Role, performerRole);

        user.ChangeRole(role);
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return UserDTO.Create(user);
    }

    public async Task<UserDTO> DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        if (user == null)
            throw new AppValidationException($"No se puede eliminar: El usuario con ID {id} no existe.", "USER_NOT_FOUND");

        await _userRepository.DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync();
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
