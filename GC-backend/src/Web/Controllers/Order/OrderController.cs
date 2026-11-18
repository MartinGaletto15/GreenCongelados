using Application.Interfaces.Order;
using Application.DTOs;
using Application.DTOs.Requests;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;

namespace Web.Controllers.Order;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderReadOnlyService _readOnlyService;
    private readonly IOrderWriteService _writeService;

    public OrderController(
        IOrderReadOnlyService readOnlyService,
        IOrderWriteService writeService)
    {
        _readOnlyService = readOnlyService;
        _writeService = writeService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetMyOrders()
    {
        var userId = User.GetUserId();
        var result = await _readOnlyService.GetMyOrdersAsync(userId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDTO>> Create([FromBody] CreateOrderRequest request)
    {
        var userId = User.GetUserId();
        var result = await _writeService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetOrderById), new { id = result.IdOrder }, result);
    }

    [HttpPut("me/{id}")]
    public async Task<ActionResult<OrderDTO>> UpdateMyOrder(int id, [FromBody] UpdateOrderRequest request)
    {
        var userId = User.GetUserId();
        var result = await _writeService.UpdateMyOrderAsync(id, request, userId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDTO>> GetOrderById(int id)
    {
        var userId = User.GetUserId();
        var role = User.GetRole();
        // Service internally validates if the user can access this order based on their ID and Role
        var result = await _readOnlyService.GetOrderByIdAsync(id, userId, role);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<OrderDTO>> CancelOrder(int id)
    {
        var userId = User.GetUserId();
        var role = User.GetRole();
        // Service handles whether it's a user canceling or an admin deleting
        var result = await _writeService.DeleteAsync(id, userId, role);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminOrSuperAdmin)]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetAllOrders()
    {
        var result = await _readOnlyService.GetAllOrdersAsync();
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = Roles.AdminOrSuperAdmin)]
    public async Task<ActionResult<OrderDTO>> UpdateStatus(int id, [FromBody] OrderStatus status)
    {
        var result = await _writeService.UpdateOrderStatusAsync(id, status);
        return Ok(result);
    }
}