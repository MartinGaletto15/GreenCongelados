using Application.Interfaces;
using Domain.Interfaces;
using Domain.Entities;
using Domain.Entities.Enums;
using Application.DTOs.Requests;
using Application.Interfaces.PasswordReset;
using Application.Interfaces.Security;
using Application.Interfaces.EmailSender;
using Microsoft.Extensions.Configuration;
using Application.Interfaces.EmailVerification;
using Domain.Exceptions;

namespace Application.Services;

public class EmailVerificationService : IEmailVerificationService
{

    private readonly IUserRepository _userRepository;
    private readonly IEmailSenderService _emailSenderService;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IUserTokenProvider _userTokenProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly int _emailConfirmationExpirationMinutes;

    public EmailVerificationService(
        IUserRepository userRepository,
        IUserTokenRepository userTokenRepository,
        IEmailSenderService emailSender,
        IUserTokenProvider userTokenProvider,
        IUnitOfWork unitOfWork,
        IConfiguration configuration
    )
    {
        _userRepository = userRepository;
        _userTokenRepository = userTokenRepository;
        _emailSenderService = emailSender;
        _userTokenProvider = userTokenProvider;
        _unitOfWork = unitOfWork;

        if (!int.TryParse(configuration["UserTokens:EmailConfirmationExpirationMinutes"], out int emailConfirmationExpirationMinutes)
            || emailConfirmationExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("UserTokens:emailConfirmationExpirationMinutes debe configurarse con un valor mayor que cero.");
        }

        _emailConfirmationExpirationMinutes = emailConfirmationExpirationMinutes;
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

    public async Task ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        // Hashear el token recibido y buscarlo en la BD
        string hashToken = _userTokenProvider.HashToken(request.Token);

        // Obtener y validar token (que exista, no esté usado y no haya expirado)
        UserToken? validToken = await FindValidTokenAsync(hashToken);
        if (validToken is null)
        {
            throw new AppValidationException("El token es inválido o ha expirado.", "INVALID_OR_EXPIRED_TOKEN");
        }

        // Obtener usuario asociado
        User? user = await _userRepository.GetByIdAsync(validToken.IdUser);
        if (user is null)
        {
            throw new AppValidationException("El token es inválido o ha expirado.", "INVALID_OR_EXPIRED_TOKEN");
        }

        // Cambiar el estado del usuario a confirmado
        user.EmailConfirm();
        validToken.MarkAsUsed();

        await _userRepository.UpdateAsync(user);
        await _userTokenRepository.UpdateAsync(validToken);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<UserToken> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new AppValidationException("El token es inválido o ha expirado.", "INVALID_OR_EXPIRED_TOKEN");
        }

        string hashToken = _userTokenProvider.HashToken(token);
        UserToken? validToken = await FindValidTokenAsync(hashToken);
        return validToken ?? throw new AppValidationException("El token es inválido o ha expirado.", "INVALID_OR_EXPIRED_TOKEN");
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