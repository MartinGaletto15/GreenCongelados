using Application.Interfaces.UserServices;
using Application.Interfaces.User;
using Application.Interfaces.Order;
using Application.DTOs;
using Application.DTOs.Requests;
using Domain.Entities.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Web.Extensions;

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

    [HttpGet("me")]
    public async Task<ActionResult<UserDTO>> GetMyUserAsync()
    {
        var userId = User.GetUserId();
        var user = await _userReadOnlyService.GetUserByIdAsync(userId);
        return Ok(user);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDTO>> UpdateUserAsync(UpdateUserRequest request)
    {
        var userId = User.GetUserId();
        var user = await _userWriteService.UpdateUserAsync(userId, request);
        return Ok(user);
    }

    [HttpDelete("me")]
    public async Task<ActionResult> DeleteUserAsync()
    {
        var userId = User.GetUserId();
        await _userWriteService.DeleteUserAsync(userId);
        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDTO>> GetUserByIdAsync([FromRoute] int id)
    {
        var currentUserId = User.GetUserId();
        var role = User.GetRole();

        if (currentUserId != id && !Roles.IsAdmin(role))
        {
            throw new AppValidationException("No tienes permiso para ver este usuario.", "FORBIDDEN", 403);
        }

        var user = await _userReadOnlyService.GetUserByIdAsync(id);
        return Ok(user);
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminOrSuperAdmin)]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsersAsync()
    {
        var users = await _userReadOnlyService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPatch("{id}/role")]
    [Authorize(Roles = Roles.AdminOrSuperAdmin)]
    public async Task<ActionResult<UserDTO>> ChangeUserRoleAsync([FromRoute] int id, [FromQuery] Role role)
    {
        // [Authorize] ya garantiza que el claim contiene un rol válido.
        var performerRole = User.GetRole();

        var user = await _userWriteService.UpdateUserRoleAsync(id, role, performerRole);
        return Ok(user);
    }

    [HttpGet("cadets")]
    [Authorize(Roles = Roles.AdminSuperAdminOrCadet)]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllCadets()
    {
        var cadets = await _cadetReadOnlyService.GetAllCadetsAsync();
        return Ok(cadets);
    }

    [HttpGet("cadets/{id}")]
    [Authorize(Roles = Roles.AdminSuperAdminOrCadet)]
    public async Task<ActionResult<UserDTO>> GetCadetById(int id)
    {
        var cadet = await _cadetReadOnlyService.GetCadetByIdAsync(id);
        return Ok(cadet);
    }

    [HttpGet("cadets/me/orders")]
    [Authorize(Roles = Roles.Cadet)]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetMyCadetOrders()
    {
        var idCadet = User.GetUserId();
        var orders = await _orderReadOnlyService.GetOrdersByCourierAsync(idCadet);
        return Ok(orders);
    }

    [HttpPut("cadets/{id}/orders/{idOrder}")]
    [Authorize(Roles = Roles.AdminOrSuperAdmin)]
    public async Task<ActionResult<UserDTO>> AssignOrderToCadet(int id, int idOrder)
    {
        var cadet = await _cadetWriteService.AsingOrderToCadetAsync(id, idOrder);
        return Ok(cadet);
    }

    [HttpPatch("cadets/me/orders/{idOrder}/status")]
    [Authorize(Roles = Roles.Cadet)]
    public async Task<ActionResult<OrderDTO>> CadetUpdateOrderStatus(int idOrder, [FromBody] OrderStatus status)
    {
        var idCadet = User.GetUserId();
        var order = await _cadetWriteService.UpdateOrderStatusAsync(idCadet, idOrder, status);
        return Ok(order);
    }
}
