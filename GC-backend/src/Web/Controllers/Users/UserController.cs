using System.Security.Claims;
using Aplication.Interfaces.UserServices;
using Applications.dtos;
using Applications.dtos.Requests;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDTO>> GetUserByIdAsync([FromRoute] int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";

        if (currentUserId != id && role != "ADMIN" && role != "SUPERADMIN")
        {
            return Forbid();
        }

        var user = await _userReadOnlyService.GetUserByIdAsync(id);
        return Ok(user);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDTO>> GetMyUserAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userReadOnlyService.GetUserByIdAsync(userId);
        return Ok(user);
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserDTO>> UpdateUserAsync(UpdateUserRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userWriteService.UpdateUserAsync(userId, request);
        return Ok(user);
    }

    [Authorize]
    [HttpDelete("me")]
    public async Task<ActionResult> DeleteUserAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _userWriteService.DeleteUserAsync(userId);
        return NoContent();
    }
}