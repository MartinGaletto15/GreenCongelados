using Aplication.Interfaces.UserServices;
using Applications.dtos.Requests;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<User>> GetUserByIdAsync(int id)
    {
        var user = await _userReadOnlyService.GetUserByIdAsync(id);
        return Ok(user);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsersAsync()
    {
        var users = await _userReadOnlyService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPut]
    public async Task<ActionResult<User>> UpdateUserAsync(UpdateUserRequest request)
    {
        var user = await _userWriteService.UpdateUserAsync(request);
        return Ok(user);
    }

    [HttpDelete]
    public async Task<ActionResult<User>> DeleteUserAsync(int id)
    {
        var user = await _userWriteService.DeleteUserAsync(id);
        return NoContent();
    }
}