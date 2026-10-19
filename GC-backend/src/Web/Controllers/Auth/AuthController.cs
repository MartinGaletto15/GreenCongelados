using Application.Interfaces.UserServices;
using Application.DTOs.Requests;
using Application.DTOs;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.PasswordReset;

namespace Web.Controllers.Auth;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserWriteService _userWriteService;
    private readonly IPasswordResetService _passwordResetService;
    
    public AuthController(IUserWriteService userWriteService, IPasswordResetService passwordResetService)
    {
        _userWriteService = userWriteService;
        _passwordResetService = passwordResetService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> RegisterAsync(CreateUserRequest request)
    {
        var user = await _userWriteService.CreateUserAsync(request);
        return Ok(user);
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
        return Ok(new { message = "Si el correo existe, se envió el código." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        await _passwordResetService.ResetPasswordAsync(request);
        return Ok();
    }
}