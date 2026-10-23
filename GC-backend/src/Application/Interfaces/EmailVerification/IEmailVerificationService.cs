using Application.DTOs.Requests;
using Domain.Entities;

namespace Application.Interfaces.EmailVerification;

public interface IEmailVerificationService
{
    Task RequestEmailVerificationAsync(ResendEmailConfirmationRequest request);
    Task SendVerificationEmailAsync(Domain.Entities.User user);
    Task ConfirmEmailAsync(ConfirmEmailRequest request);
    Task<UserToken> ValidateTokenAsync(string token);
}