using Aplication.Interfaces.Order;
using Applications.dtos;
using Applications.dtos.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Orders;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetAllAsync()
    {
        var result = await _orderService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDTO>> GetByIdAsync(int id)
    {
        var result = await _orderService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDTO>> CreateAsync(CreateOrderRequest request)
    {
        var result = await _orderService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<OrderDTO>> UpdateAsync(int id, UpdateOrderRequest request)
    {
        var result = await _orderService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        await _orderService.DeleteAsync(id);
        return NoContent();
    }
}