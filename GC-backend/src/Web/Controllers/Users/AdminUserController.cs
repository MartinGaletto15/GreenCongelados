using Aplication.Interfaces.UserServices;
using Applications.dtos;
using Domain.Entities;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers.Users;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "ADMIN,SUPERADMIN")]
public class AdminUserController : ControllerBase
{
    private readonly IUserReadOnlyService _userReadOnlyService;
    private readonly IUserWriteService _userWriteService;

    public AdminUserController(IUserReadOnlyService userReadOnlyService, IUserWriteService userWriteService)
    {
        _userReadOnlyService = userReadOnlyService;
        _userWriteService = userWriteService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsersAsync()
    {
        var users = await _userReadOnlyService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPatch("{id}/role")]
    public async Task<ActionResult<UserDTO>> ChangeUserRoleAsync([FromRoute] int id, [FromQuery] Role role)
    {
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (!Enum.TryParse<Role>(roleClaim, out var performerRole))
        {
            return Unauthorized();
        }

        var user = await _userWriteService.UpdateUserRoleAsync(id, role, performerRole);
        return Ok(user);
    }
}