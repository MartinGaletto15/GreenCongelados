using Application.Interfaces;
using Domain.Interfaces;
using Domain.Entities;
using Domain.Entities.Enums;
using Application.DTOs;
using Application.DTOs.Requests;
using Application.Interfaces.PasswordReset;
using Application.Interfaces.Security;
using Application.Interfaces.EmailSender;
using Application.Configuration;
using Microsoft.Extensions.Options;
using Application.Interfaces.EmailVerification;
using Domain.Exceptions;
using Application.Validators;

namespace Application.Services;

public class EmailVerificationService : IEmailVerificationService
{

    private readonly IUserRepository _userRepository;
    private readonly IEmailSenderService _emailSenderService;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IUserTokenProvider _userTokenProvider;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly int _emailConfirmationExpirationMinutes;

    public EmailVerificationService(
        IUserRepository userRepository,
        IUserTokenRepository userTokenRepository,
        IEmailSenderService emailSender,
        IUserTokenProvider userTokenProvider,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IOptions<UserTokenOptions> userTokenOptions
    )
    {
        _userRepository = userRepository;
        _userTokenRepository = userTokenRepository;
        _emailSenderService = emailSender;
        _userTokenProvider = userTokenProvider;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _emailConfirmationExpirationMinutes = userTokenOptions.Value.EmailConfirmationExpirationMinutes;
    }

    public async Task RequestEmailVerificationAsync(ResendEmailConfirmationRequest request)
    {
        // Buscar usuario por email
        User? user = await _userRepository.GetByEmailAsync(request.Email);

        // Si no existe o ya está confirmado, salir sin error (anti-enumeración)
        if (user == null || user.EmailConfirmed)
        {
            return;
        }

        await SendVerificationEmailAsync(user);
    }

    /// <summary>
    /// Primitivo interno: genera y envía el token para un usuario ya conocido.
    /// Para el flujo público de reenvío (anti-enumeración), usar
    /// <see cref="RequestEmailVerificationAsync"/>.
    /// </summary>
    public async Task SendVerificationEmailAsync(User user)
    {
        // Si ya está confirmado, no generar un nuevo token
        if (user.EmailConfirmed)
        {
            return;
        }

        // Invalidar tokens previos de confirmación
        await _userTokenRepository.InvalidateAllForUserAsync(user.IdUser, TokenType.EmailConfirmation);

        // Generar nuevo token seguro y persistirlo
        var secureToken = _userTokenProvider.GenerateToken();
        var hashToken = _userTokenProvider.HashToken(secureToken);

        var confirmationToken = new UserToken(
            user.IdUser,
            hashToken,
            _emailConfirmationExpirationMinutes,
            TokenType.EmailConfirmation
        );

        await _userTokenRepository.AddAsync(confirmationToken);
        await _unitOfWork.SaveChangesAsync();

        // Enviar correo de confirmación (después de persistir el token y sin
        // cancelación: si el envío falla, el usuario puede solicitar un reenvío)
        await _emailSenderService.SendEmailConfirmationAsync(user.Email, secureToken);
    }

    public async Task<EmailConfirmationTokenDTO> ValidateEmailConfirmationTokenAsync(ConfirmEmailRequest request)
    {
        // Solo se valida el token y que el usuario asociado exista.
        // El email no se confirma ni el token se consume; eso ocurre en
        // CompleteRegistrationAsync, cuando también se define la contraseña.
        UserToken validToken = await ValidateTokenAsync(request.Token);

        User? user = await _userRepository.GetByIdAsync(validToken.IdUser);
        if (user is null)
        {
            throw new AppValidationException("El código no es válido o venció. Solicita uno nuevo.", "INVALID_OR_EXPIRED_TOKEN");
        }

        return new EmailConfirmationTokenDTO(user.Email);
    }

    public async Task CompleteRegistrationAsync(CompleteRegistrationRequest request)
    {
        UserValidator.ValidateCompleteRegistration(request);

        // El token se vuelve a validar acá: es la validación que realmente importa,
        // porque es la que habilita la escritura.
        UserToken validToken = await ValidateTokenAsync(request.Token);

        User? user = await _userRepository.GetByIdAsync(validToken.IdUser);
        if (user is null)
        {
            throw new AppValidationException("El código no es válido o venció. Solicita uno nuevo.", "INVALID_OR_EXPIRED_TOKEN");
        }

        string passwordHash = _passwordHasher.Hash(request.Password);

        // Confirmar email, guardar contraseña y consumir el token en una sola transacción.
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            user.CompleteRegistration(request.Name, request.LastName, passwordHash);
            validToken.MarkAsUsed();

            await _userRepository.UpdateAsync(user);
            await _userTokenRepository.UpdateAsync(validToken);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private async Task<UserToken> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new AppValidationException("El código no es válido o venció. Solicita uno nuevo.", "INVALID_OR_EXPIRED_TOKEN");
        }

        string hashToken = _userTokenProvider.HashToken(token);
        UserToken? validToken = await FindValidTokenAsync(hashToken);
        return validToken ?? throw new AppValidationException("El código no es válido o venció. Solicita uno nuevo.", "INVALID_OR_EXPIRED_TOKEN");
    }

    private async Task<UserToken?> FindValidTokenAsync(string hashToken)
    {
        UserToken? userToken = await _userTokenRepository.GetByTokenAsync(hashToken, TokenType.EmailConfirmation);
        if (userToken is null || userToken.IsExpired() || userToken.IsUsed)
        {
            return null;
        }

        return userToken;
    }

}