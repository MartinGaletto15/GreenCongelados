using Microsoft.AspNetCore.Mvc;
using Aplication.Interfaces.User;
using Aplication.Interfaces.Order;
using Applications.dtos;
using Microsoft.AspNetCore.Authorization;
using Aplication.Interfaces.UserServices;
using System.Security.Claims;
using Domain.Enums.Entities;

namespace Web.Controllers.Users;

[ApiController]
[Route("api/users/cadet")]
[Authorize(Roles = "ADMIN,SUPERADMIN,CADET")]
public class CadetUserController : ControllerBase
{
    private readonly ICadetReadOnlyService _cadetReadOnlyService;
    private readonly ICadetWriteService _cadetWriteService;
    private readonly IOrderReadOnlyService _orderReadOnlyService;
    
    public CadetUserController(
        ICadetReadOnlyService cadetReadOnlyService, 
        ICadetWriteService cadetWriteService,
        IOrderReadOnlyService orderReadOnlyService)
    {
        _cadetReadOnlyService = cadetReadOnlyService;
        _cadetWriteService = cadetWriteService;
        _orderReadOnlyService = orderReadOnlyService;
    }

    [HttpGet("my-orders")]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetMyOrders()
    {
        var idCadet = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var orders = await _orderReadOnlyService.GetOrdersByCourierAsync(idCadet);
        return Ok(orders);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllCadets()
    {
        var cadets = await _cadetReadOnlyService.GetAllCadetsAsync();
        return Ok(cadets);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDTO>> GetCadetById(int id)
    {
        var cadet = await _cadetReadOnlyService.GetCadetByIdAsync(id);
        return Ok(cadet);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,SUPERADMIN")]
    public async Task<ActionResult<UserDTO>> AsingOrderToCadet(int id, int idOrder)
    {
        var cadet = await _cadetWriteService.AsingOrderToCadetAsync(id, idOrder);
        return Ok(cadet);
    }

    [HttpPatch("order/{idOrder}/status")]
    public async Task<ActionResult<OrderDTO>> UpdateOrderStatus(int idOrder, [FromBody] OrderStatus status)
    {
        var idCadet = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await _cadetWriteService.UpdateOrderStatusAsync(idCadet, idOrder, status);
        return Ok(order);
    }
}