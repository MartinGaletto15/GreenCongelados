using Application.DTOs.Requests;
using Domain.Entities;

namespace Application.Interfaces.PasswordReset;

public interface IPasswordResetService
{
    Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task<UserToken> ValidateTokenAsync(string token);
}