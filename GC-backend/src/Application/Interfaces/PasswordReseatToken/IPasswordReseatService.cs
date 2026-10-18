using Application.DTOs.Requests;

namespace Application.Interfaces.PasswordReseat;

public interface IPasswordReseatService
{
    Task RequestPasswordResetAsync(ForgotPasswordRequest request);
    Task PasswordReseatAsync(ResetPasswordRequest request);
    Task<bool> ValidateTokenAsync(string token);
}