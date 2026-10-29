using Application.Interfaces.UserServices;
using Application.DTOs.Requests;
using Application.DTOs;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.PasswordReset;
using Application.Interfaces.EmailVerification;

namespace Web.Controllers.Auth;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserWriteService _userWriteService;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IEmailVerificationService _emailVerificationService;
    
    public AuthController(
        IUserWriteService userWriteService,
        IPasswordResetService passwordResetService,
        IEmailVerificationService emailVerificationService)
    {
        _userWriteService = userWriteService;
        _passwordResetService = passwordResetService;
        _emailVerificationService = emailVerificationService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(CreateUserRequest request)
    {
        await _userWriteService.CreateUserAsync(request);
        return StatusCode(
            StatusCodes.Status201Created,
            new { message = "Te enviamos un código de verificación. Revisa tu correo y completa tu registro." });
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> LoginAsync(LoginRequest request)
    {
        var token = await _userWriteService.LoginAsync(request);
        return Ok(new { Token = token });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
    {
        await _passwordResetService.RequestPasswordResetAsync(request);
        return Ok(new { message = "Si el email está registrado, te enviamos un código." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        await _passwordResetService.ResetPasswordAsync(request);
        return Ok();
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<EmailConfirmationTokenDTO>> VerifyEmailAsync([FromBody] ConfirmEmailRequest request)
    {
        var result = await _emailVerificationService.ValidateEmailConfirmationTokenAsync(request);
        return Ok(result);
    }

    [HttpPost("complete-registration")]
    public async Task<IActionResult> CompleteRegistrationAsync([FromBody] CompleteRegistrationRequest request)
    {
        await _emailVerificationService.CompleteRegistrationAsync(request);
        return Ok();
    }

    [HttpPost("resend-verification-email")]
    public async Task<IActionResult> ResendVerificationEmailAsync([FromBody] ResendEmailConfirmationRequest request)
    {
        await _emailVerificationService.RequestEmailVerificationAsync(request);
        return Ok(new { message = "Si el email está registrado y no está verificado, te enviamos un código." });
    }
}