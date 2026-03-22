using Aplication.Interfaces.Order;
using Applications.dtos;
using Applications.dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers.Order;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet("my-order")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetMyOrders()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _readOnlyService.GetMyOrdersAsync(userId);
        return Ok(result);
    }

    [HttpGet("my-order/{id}")]
    [Authorize]
    public async Task<ActionResult<OrderDTO>> GetMyOrderById(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var result = await _readOnlyService.GetMyOrderByIdAsync(id, userId, role);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<OrderDTO>> Create([FromBody] CreateOrderRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _writeService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetMyOrderById), new { id = result.IdOrder }, result);
    }

    [HttpPut("my-order/{id}")]
    [Authorize]
    public async Task<ActionResult<OrderDTO>> UpdateMyOrder(int id, [FromBody] UpdateOrderRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _writeService.UpdateMyOrderAsync(id, request, userId);
        return Ok(result);
    }

    [HttpDelete("my-order/{id}")]
    [Authorize]
    public async Task<ActionResult<OrderDTO>> CancelOrder(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var result = await _writeService.DeleteAsync(id, userId, role);
        return Ok(result);
    }
}