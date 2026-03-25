using Aplication.Interfaces.Order;
using Applications.dtos;
using Applications.dtos.Requests;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    // --- CLIENT ENDPOINTS ---

    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetMyOrders()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _readOnlyService.GetMyOrdersAsync(userId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDTO>> Create([FromBody] CreateOrderRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _writeService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetOrderById), new { id = result.IdOrder }, result);
    }

    [HttpPut("me/{id}")]
    public async Task<ActionResult<OrderDTO>> UpdateMyOrder(int id, [FromBody] UpdateOrderRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _writeService.UpdateMyOrderAsync(id, request, userId);
        return Ok(result);
    }

    // --- SHARED ENDPOINTS ---

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDTO>> GetOrderById(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        // El servicio valida internamente si el usuario puede acceder a este pedido basado en su ID y Rol
        var result = await _readOnlyService.GetOrderByIdAsync(id, userId, role);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<OrderDTO>> CancelOrder(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        // El servicio maneja si es un usuario cancelando o un admin eliminando
        var result = await _writeService.DeleteAsync(id, userId, role);
        return Ok(result);
    }

    // --- ADMIN ENDPOINTS ---

    [HttpGet]
    [Authorize(Roles = "ADMIN,SUPERADMIN")]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetAllOrders()
    {
        var result = await _readOnlyService.GetAllOrdersAsync();
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "ADMIN,SUPERADMIN")]
    public async Task<ActionResult<OrderDTO>> UpdateStatus(int id, [FromBody] OrderStatus status)
    {
        var result = await _writeService.UpdateOrderStatusAsync(id, status);
        return Ok(result);
    }
}