using System.Security.Claims;
using Aplication.Interfaces.UserServices;
using Aplication.Interfaces.User;
using Aplication.Interfaces.Order;
using Applications.dtos;
using Applications.dtos.Requests;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers.Users;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserWriteService _userWriteService;
    private readonly IUserReadOnlyService _userReadOnlyService;
    private readonly ICadetReadOnlyService _cadetReadOnlyService;
    private readonly ICadetWriteService _cadetWriteService;
    private readonly IOrderReadOnlyService _orderReadOnlyService;

    public UserController(
        IUserWriteService userWriteService, 
        IUserReadOnlyService userReadOnlyService,
        ICadetReadOnlyService cadetReadOnlyService,
        ICadetWriteService cadetWriteService,
        IOrderReadOnlyService orderReadOnlyService)
    {
        _userWriteService = userWriteService;
        _userReadOnlyService = userReadOnlyService;
        _cadetReadOnlyService = cadetReadOnlyService;
        _cadetWriteService = cadetWriteService;
        _orderReadOnlyService = orderReadOnlyService;
    }

    // --- PROFILE ENDPOINTS ---

    [HttpGet("me")]
    public async Task<ActionResult<UserDTO>> GetMyUserAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userReadOnlyService.GetUserByIdAsync(userId);
        return Ok(user);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDTO>> UpdateUserAsync(UpdateUserRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userWriteService.UpdateUserAsync(userId, request);
        return Ok(user);
    }

    [HttpDelete("me")]
    public async Task<ActionResult> DeleteUserAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _userWriteService.DeleteUserAsync(userId);
        return NoContent();
    }

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

    // --- ADMIN USER ENDPOINTS ---

    [HttpGet]
    [Authorize(Roles = "ADMIN,SUPERADMIN")]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsersAsync()
    {
        var users = await _userReadOnlyService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPatch("{id}/role")]
    [Authorize(Roles = "ADMIN,SUPERADMIN")]
    public async Task<ActionResult<UserDTO>> ChangeUserRoleAsync([FromRoute] int id, [FromQuery] Role role)
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (!Enum.TryParse<Role>(roleClaim, out var performerRole))
        {
            return Unauthorized();
        }

        var user = await _userWriteService.UpdateUserRoleAsync(id, role, performerRole);
        return Ok(user);
    }

    // --- CADET ENDPOINTS ---

    [HttpGet("cadets")]
    [Authorize(Roles = "ADMIN,SUPERADMIN,CADET")]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllCadets()
    {
        var cadets = await _cadetReadOnlyService.GetAllCadetsAsync();
        return Ok(cadets);
    }

    [HttpGet("cadets/{id}")]
    [Authorize(Roles = "ADMIN,SUPERADMIN,CADET")]
    public async Task<ActionResult<UserDTO>> GetCadetById(int id)
    {
        var cadet = await _cadetReadOnlyService.GetCadetByIdAsync(id);
        return Ok(cadet);
    }

    [HttpGet("cadets/me/orders")]
    [Authorize(Roles = "CADET")]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetMyCadetOrders()
    {
        var idCadet = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var orders = await _orderReadOnlyService.GetOrdersByCourierAsync(idCadet);
        return Ok(orders);
    }

    [HttpPut("cadets/{id}/orders/{idOrder}")]
    [Authorize(Roles = "ADMIN,SUPERADMIN")]
    public async Task<ActionResult<UserDTO>> AssignOrderToCadet(int id, int idOrder)
    {
        var cadet = await _cadetWriteService.AsingOrderToCadetAsync(id, idOrder);
        return Ok(cadet);
    }

    [HttpPatch("cadets/me/orders/{idOrder}/status")]
    [Authorize(Roles = "CADET")]
    public async Task<ActionResult<OrderDTO>> CadetUpdateOrderStatus(int idOrder, [FromBody] OrderStatus status)
    {
        var idCadet = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await _cadetWriteService.UpdateOrderStatusAsync(idCadet, idOrder, status);
        return Ok(order);
    }
}