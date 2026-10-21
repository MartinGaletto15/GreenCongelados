using Application.DTOs.Requests;
using Domain.Entities;

namespace Application.Interfaces.EmailVerification;

public interface IEmailVerificationService
{
    Task RequestEmailVerificationAsync(ResendEmailConfirmationRequest request, CancellationToken cancellationToken = default);
    Task SendVerificationEmailAsync(Domain.Entities.User user, CancellationToken cancellationToken = default);
    Task ConfirmEmailAsync(ConfirmEmailRequest request);
    Task<UserToken> ValidateTokenAsync(string token);
}