using Aplication.Interfaces.Order;
using Applications.dtos;
using Domain.Enums.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GC.Web.Controllers.Order;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminOrderController : ControllerBase
{
    private readonly IOrderReadOnlyService _readOnlyService;
    private readonly IOrderWriteService _writeService;

    public AdminOrderController(IOrderReadOnlyService readOnlyService, IOrderWriteService writeService)
    {
        _readOnlyService = readOnlyService;
        _writeService = writeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetAllOrders()
    {
        var result = await _readOnlyService.GetAllOrdersAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin,Cadet")]
    public async Task<ActionResult<OrderDTO>> GetOrderById(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var result = await _readOnlyService.GetOrderByIdAsync(id, userId, role);
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<OrderDTO>> UpdateStatus(int id, [FromBody] OrderStatus status)
    {
        var result = await _writeService.UpdateOrderStatusAsync(id, status);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<OrderDTO>> Delete(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var result = await _writeService.DeleteAsync(id, userId, role);
        return Ok(result);
    }
}