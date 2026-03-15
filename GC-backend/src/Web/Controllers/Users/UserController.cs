using System.Security.Claims;
using Aplication.Interfaces.UserServices;
using Applications.dtos.Requests;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Users;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserWriteService _userWriteService;
    private readonly IUserReadOnlyService _userReadOnlyService;

    public UserController(IUserWriteService userWriteService, IUserReadOnlyService userReadOnlyService)
    {
        _userWriteService = userWriteService;
        _userReadOnlyService = userReadOnlyService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserByIdAsync([FromRoute] int id)
    {
        var user = await _userReadOnlyService.GetUserByIdAsync(id);
        return Ok(user);
    }

    [HttpGet("me")]
    public async Task<ActionResult<User>> GetMyUserAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userReadOnlyService.GetUserByIdAsync(userId);
        return Ok(user);
    }

    [HttpPut("me")]
    public async Task<ActionResult<User>> UpdateUserAsync(UpdateUserRequest request)
    {
        var user = await _userWriteService.UpdateUserAsync(request);
        return Ok(user);
    }

    [HttpDelete("me")]
    public async Task<ActionResult> DeleteUserAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _userWriteService.DeleteUserAsync(userId);
        return NoContent();
    }
}