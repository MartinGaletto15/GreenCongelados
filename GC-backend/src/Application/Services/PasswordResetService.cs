using Application.Interfaces;
using Domain.Interfaces;
using Domain.Entities;
using Application.DTOs.Requests;
using Application.Interfaces.PasswordReset;
using Application.Interfaces.Security;
using Application.Interfaces.EmailSender;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class PasswordResetService : IPasswordResetService
{

    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IEmailSenderService _emailSenderService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordResetTokenProvider _passwordResetTokenProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly int _tokenExpirationMinutes;

    public PasswordResetService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailSenderService emailSender,
        IPasswordHasher passwordHasher,
        IPasswordResetTokenProvider passwordResetTokenProvider,
        IUnitOfWork unitOfWork,
        IConfiguration configuration
    )
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailSenderService = emailSender;
        _passwordHasher = passwordHasher;
        _passwordResetTokenProvider = passwordResetTokenProvider;
        _unitOfWork = unitOfWork;

        if (!int.TryParse(configuration["PasswordReset:TokenExpirationMinutes"], out int tokenExpirationMinutes)
            || tokenExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("PasswordReset:TokenExpirationMinutes debe configurarse con un valor mayor que cero.");
        }

        _tokenExpirationMinutes = tokenExpirationMinutes;
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
        await _passwordResetTokenRepository.InvalidateAllForUserAsync(user.IdUser);
        // Generar nuevo token seguro y guardarlo
        var secureToken = _passwordResetTokenProvider.GenerateToken();
        var hashToken = _passwordResetTokenProvider.HashToken(secureToken);
        var resetToken = new PasswordResetToken(user.IdUser, hashToken, _tokenExpirationMinutes);
        await _passwordResetTokenRepository.AddAsync(resetToken);
        await _unitOfWork.SaveChangesAsync();
        // Enviar email con el enlace
        await _emailSenderService.SendPasswordResetEmailAsync(user.Email, secureToken);

    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        // Hashear el token recibido y buscarlo en la BD
        string hashToken = _passwordResetTokenProvider.HashToken(request.Token);

        // Obtener y validar token (que exista, no esté usado y no haya expirado)
        PasswordResetToken? validToken = await FindValidTokenAsync(hashToken);
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
        await _passwordResetTokenRepository.UpdateAsync(validToken);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PasswordResetToken> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("El token es inválido o ha expirado.");
        }

        string hashToken = _passwordResetTokenProvider.HashToken(token);
        PasswordResetToken? validToken = await FindValidTokenAsync(hashToken);
        return validToken ?? throw new InvalidOperationException("El token es inválido o ha expirado.");
    }

    private async Task<PasswordResetToken?> FindValidTokenAsync(string hashToken)
    {
        PasswordResetToken? resetToken = await _passwordResetTokenRepository.GetByTokenAsync(hashToken);
        if (resetToken is null || resetToken.IsExpired() || resetToken.IsUsed)
        {
            return null;
        }

        return resetToken;
    }

}