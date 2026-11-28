using Application.DTOs;
using Application.DTOs.Requests;
using Domain.Entities;

namespace Application.Interfaces.EmailVerification;

public interface IEmailVerificationService
{
    Task RequestEmailVerificationAsync(ResendEmailConfirmationRequest request);
    Task SendVerificationEmailAsync(Domain.Entities.User user);
    Task<EmailConfirmationTokenDTO> ValidateEmailConfirmationTokenAsync(ConfirmEmailRequest request);
    Task CompleteRegistrationAsync(CompleteRegistrationRequest request);
}