using System.Security.Claims;
using Application.DTOs;
using Application.DTOs.Requests;
using Application.Validators;
using Application.Interfaces.Security;
using Application.Interfaces.UserServices;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Application.Interfaces.CartItem;
using Application.Interfaces.EmailSender;
using Application.Interfaces.EmailVerification;


namespace Application.Services;

public class UserService : IUserWriteService, IUserReadOnlyService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserTokenProvider _userTokenProvider;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly ICartItemWriteService _cartItemWriteService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IEmailSenderService _emailSenderService;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IRevokedUserStore _revokedUserStore;

    public UserService(
        IUserRepository userRepository,
        IUserTokenProvider userTokenProvider,
        IConfiguration configuration,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        ICartItemWriteService cartItemWriteService,
        IUnitOfWork unitOfWork,
        IOrderRepository orderRepository,
        IUserTokenRepository userTokenRepository,
        IEmailSenderService emailSenderService,
        IEmailVerificationService emailVerificationService,
        IRevokedUserStore revokedUserStore)
    {
        _userRepository = userRepository;
        _userTokenProvider = userTokenProvider;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _cartItemWriteService = cartItemWriteService;
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _userTokenRepository = userTokenRepository;
        _emailSenderService = emailSenderService;
        _emailVerificationService = emailVerificationService;
        _revokedUserStore = revokedUserStore;
    }

    public async Task<string> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
            throw new AppValidationException("Email o contraseña incorrectos.", "AUTH_INVALID_CREDENTIALS");

        if (!user.EmailConfirmed || user.Password == null)
            throw new AppValidationException("Tu cuenta todavía no está confirmada. Revisa tu correo y completa el registro.", "USER_EMAIL_NOT_CONFIRMED");

        if (!_passwordHasher.Verify(request.Password, user.Password))
            throw new AppValidationException("Email o contraseña incorrectos.", "AUTH_INVALID_CREDENTIALS");

        return _jwtProvider.GenerateToken(user);
    }

    public async Task<UserDTO> CreateUserAsync(CreateUserRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        UserValidator.ValidateCreate(request, existingUser);
        User user;

        // Usuario y carrito se confirman juntos; el correo se envía después del commit.
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (existingUser is not null)
            {
                // Invalidar tokens de confirmación previos
                await _userTokenRepository.InvalidateAllForUserAsync(existingUser.IdUser, TokenType.EmailConfirmation);

                // Buscar usuario existente
                user = existingUser;

                // Reenviar nuevo token al final del metodo
            }
            else
            {
                // Crear nuevo usuario
                user = new User(
                    request.Email
                );
                await _userRepository.AddAsync(user);
            }

            await _unitOfWork.SaveChangesAsync();

            // Crear carrito solo si es un usuario estrictamente nuevo
            if (existingUser is null)
            {
                await _cartItemWriteService.CreateCartAsync(user.IdUser);
            }

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        // Generar token y enviar correo de confirmación (después del commit).
        // Un fallo de SMTP no revierte el registro ni invalida la petición, y el
        // envío no se cancela aunque el cliente abandone la petición.
        await _emailVerificationService.SendVerificationEmailAsync(user);

        return UserDTO.Create(user);
    }

    public async Task<UserDTO> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new AppValidationException("El usuario no existe.", "USER_NOT_FOUND");

        await UserValidator.ValidateUpdateAsync(request, _userRepository, user);

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

        // Rechazar cualquier JWT emitido para la cuenta eliminada hasta que venza.
        _revokedUserStore.Revoke(user.IdUser);

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
