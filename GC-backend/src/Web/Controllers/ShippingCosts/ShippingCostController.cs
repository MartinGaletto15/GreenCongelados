using Aplication.Interfaces.ShippingCost;
using Applications.dtos;
using Applications.dtos.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.ShippingCosts;

[ApiController]
[Route("api/shipping-costs")]
public class ShippingCostController : ControllerBase
{
    private readonly IShippingCostService _shippingCostService;

    public ShippingCostController(IShippingCostService shippingCostService)
    {
        _shippingCostService = shippingCostService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShippingCostDTO>>> GetAllAsync()
    {
        var result = await _shippingCostService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShippingCostDTO>> GetByIdAsync(int id)
    {
        var result = await _shippingCostService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ShippingCostDTO>> CreateAsync(CreateShippingCostRequest request)
    {
        var result = await _shippingCostService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ShippingCostDTO>> UpdateAsync(int id, UpdateShippingCostRequest request)
    {
        var result = await _shippingCostService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        await _shippingCostService.DeleteAsync(id);
        return NoContent();
    }
}