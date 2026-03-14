using Aplication.Interfaces.UserServices;
using Applications.dtos.Requests;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserWriteService _userWriteService;
    
    public AuthController(IUserWriteService userWriteService)
    {
        _userWriteService = userWriteService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<User>> RegisterAsync(CreateUserRequest request)
    {
        var user = await _userWriteService.CreateUserAsync(request);
        return Ok(user);
    }

    [HttpPost("login")]
    public Task<ActionResult<User>> LoginAsync(CreateUserRequest request)
    {
        // Falta implementar
        throw new AppValidationException("No se implemento el login", "LOGIN_NOT_IMPLEMENTED");
    }
}