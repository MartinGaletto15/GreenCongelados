using Application.Interfaces;
using Domain.Interfaces;
using Domain.Entities;
using Domain.Entities.Enums;
using Application.DTOs.Requests;
using Application.Interfaces.PasswordReset;
using Application.Interfaces.Security;
using Application.Interfaces.EmailSender;
using Application.Configuration;
using Microsoft.Extensions.Options;
using Domain.Exceptions;
using Application.Validators;

namespace Application.Services;

public class PasswordResetService : IPasswordResetService
{

    private readonly IUserRepository _userRepository;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IEmailSenderService _emailSenderService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserTokenProvider _userTokenProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly int _passwordResetExpirationMinutes;

    public PasswordResetService(
        IUserRepository userRepository,
        IUserTokenRepository userTokenRepository,
        IEmailSenderService emailSender,
        IPasswordHasher passwordHasher,
        IUserTokenProvider userTokenProvider,
        IUnitOfWork unitOfWork,
        IOptions<UserTokenOptions> userTokenOptions
    )
    {
        _userRepository = userRepository;
        _userTokenRepository = userTokenRepository;
        _emailSenderService = emailSender;
        _passwordHasher = passwordHasher;
        _userTokenProvider = userTokenProvider;
        _unitOfWork = unitOfWork;
        _passwordResetExpirationMinutes = userTokenOptions.Value.PasswordResetExpirationMinutes;
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request)
    {
        // Buscar usuario por email
        User? user = await _userRepository.GetByEmailAsync(request.Email);
        // No revelar si la cuenta existe ni permitir resetear cuentas sin confirmar.
        if (user == null || !user.EmailConfirmed)
        {
            return;
        }
        // Invalidar tokens previos del usuario
        await _userTokenRepository.InvalidateAllForUserAsync(user.IdUser, TokenType.PasswordReset);
        // Generar nuevo token seguro y guardarlo
        var secureToken = _userTokenProvider.GenerateToken();
        var hashToken = _userTokenProvider.HashToken(secureToken);
        var resetToken = new UserToken(user.IdUser, hashToken, _passwordResetExpirationMinutes, TokenType.PasswordReset);
        await _userTokenRepository.AddAsync(resetToken);
        await _unitOfWork.SaveChangesAsync();
        // Enviar email con el enlace (después del commit y sin cancelación:
        // si el envío falla, el usuario puede solicitar un reenvío)
        await _emailSenderService.SendPasswordResetEmailAsync(user.Email, secureToken);

    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (!Validations.ValidatePassword(request.NewPassword, 6, 20, true, true))
            throw new AppValidationException("La contraseña debe tener entre 6 y 20 caracteres e incluir al menos una mayúscula y un número.", "USER_PASSWORD_INVALID");

        // Hashear el token recibido y buscarlo en la BD
        string hashToken = _userTokenProvider.HashToken(request.Token);

        // Obtener y validar token (que exista, no esté usado y no haya expirado)
        UserToken? validToken = await FindValidTokenAsync(hashToken);
        if (validToken is null)
        {
            throw new AppValidationException("El código no es válido o venció. Solicita uno nuevo.", "INVALID_OR_EXPIRED_TOKEN");

        }

        // Obtener usuario asociado
        User? user = await _userRepository.GetByIdAsync(validToken.IdUser);
        if (user is null || !user.EmailConfirmed)
        {
            throw new AppValidationException("El código no es válido o venció. Solicita uno nuevo.", "INVALID_OR_EXPIRED_TOKEN");
        }

        // Hashear y actualizar contraseña
        string newPasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatePassword(newPasswordHash);
        validToken.MarkAsUsed();

        await _userRepository.UpdateAsync(user);
        await _userTokenRepository.UpdateAsync(validToken);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<UserToken> ValidateTokenAsync(string token)
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
        UserToken? userToken = await _userTokenRepository.GetByTokenAsync(hashToken, TokenType.PasswordReset);
        if (userToken is null || userToken.IsExpired() || userToken.IsUsed)
        {
            return null;
        }

        return userToken;
    }

}