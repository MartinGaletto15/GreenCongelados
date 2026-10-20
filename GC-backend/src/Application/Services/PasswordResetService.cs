using Application.Interfaces;
using Domain.Interfaces;
using Domain.Entities;
using Domain.Entities.Enums;
using Application.DTOs.Requests;
using Application.Interfaces.PasswordReset;
using Application.Interfaces.Security;
using Application.Interfaces.EmailSender;
using Microsoft.Extensions.Configuration;

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
        IConfiguration configuration
    )
    {
        _userRepository = userRepository;
        _userTokenRepository = userTokenRepository;
        _emailSenderService = emailSender;
        _passwordHasher = passwordHasher;
        _userTokenProvider = userTokenProvider;
        _unitOfWork = unitOfWork;

        if (!int.TryParse(configuration["UserTokens:PasswordResetExpirationMinutes"], out int passwordResetExpirationMinutes)
            || passwordResetExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("UserTokens:PasswordResetExpirationMinutes debe configurarse con un valor mayor que cero.");
        }

        _passwordResetExpirationMinutes = passwordResetExpirationMinutes;
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request)
    {
        // Buscar usuario por email
        User? user = await _userRepository.GetByEmailAsync(request.Email);
        // Si no existe, return (anti-enumeración)
        if (user == null)
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
        // Enviar email con el enlace
        await _emailSenderService.SendPasswordResetEmailAsync(user.Email, secureToken);

    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        // Hashear el token recibido y buscarlo en la BD
        string hashToken = _userTokenProvider.HashToken(request.Token);

        // Obtener y validar token (que exista, no esté usado y no haya expirado)
        UserToken? validToken = await FindValidTokenAsync(hashToken);
        if (validToken is null)
        {
            throw new InvalidOperationException("El token es inválido o ha expirado.");
        }
        
        // Obtener usuario asociado
        User? user = await _userRepository.GetByIdAsync(validToken.IdUser);
        if (user is null)
        {
            throw new InvalidOperationException("El token es inválido o ha expirado.");
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
            throw new InvalidOperationException("El token es inválido o ha expirado.");
        }

        string hashToken = _userTokenProvider.HashToken(token);
        UserToken? validToken = await FindValidTokenAsync(hashToken);
        return validToken ?? throw new InvalidOperationException("El token es inválido o ha expirado.");
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