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
    public async Task<ActionResult<UserDTO>> RegisterAsync(CreateUserRequest request)
    {
        var user = await _userWriteService.CreateUserAsync(request, HttpContext.RequestAborted);
        return StatusCode(StatusCodes.Status201Created, user);
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
        await _passwordResetService.RequestPasswordResetAsync(request, HttpContext.RequestAborted);
        return Ok(new { message = "Si el correo existe, se envió el código." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        await _passwordResetService.ResetPasswordAsync(request);
        return Ok();
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmailAsync([FromBody] ConfirmEmailRequest request)
    {
        await _emailVerificationService.ConfirmEmailAsync(request);
        return Ok();
    }

    [HttpPost("resend-verification-email")]
    public async Task<IActionResult> ResendVerificationEmailAsync([FromBody] ResendEmailConfirmationRequest request)
    {
        await _emailVerificationService.RequestEmailVerificationAsync(request, HttpContext.RequestAborted);
        return Ok(new { message = "Si el correo existe y no está verificado, se envió el código." });
    }
}