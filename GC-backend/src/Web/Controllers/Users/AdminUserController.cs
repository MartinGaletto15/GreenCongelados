using Aplication.Interfaces.UserServices;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Users;

[ApiController]
[Route("api/admin/users")]
public class AdminUserController : ControllerBase
{
    private readonly IUserReadOnlyService _userReadOnlyService;

    public AdminUserController(IUserReadOnlyService userReadOnlyService)
    {
        _userReadOnlyService = userReadOnlyService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsersAsync()
    {
        var users = await _userReadOnlyService.GetAllUsersAsync();
        return Ok(users);
    }
}