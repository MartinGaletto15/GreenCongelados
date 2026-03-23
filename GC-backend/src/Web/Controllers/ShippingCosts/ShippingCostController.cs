using Aplication.Interfaces.ShippingCost;
using Applications.dtos;
using Applications.dtos.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers.ShippingCosts;

[ApiController]
[Route("api/shipping-cost")]
[Authorize(Roles = "ADMIN,SUPERADMIN")]
public class ShippingCostController : ControllerBase
{
    private readonly IShippingCostWriteService _writeService;
    private readonly IShippingCostReadOnlyService _readService;

    public ShippingCostController(IShippingCostWriteService writeService, IShippingCostReadOnlyService readService)
    {
        _writeService = writeService;
        _readService = readService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShippingCostDTO>>> GetAllAsync()
    {
        var result = await _readService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShippingCostDTO>> GetByIdAsync(int id)
    {
        var result = await _readService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ShippingCostDTO>> CreateAsync(CreateShippingCostRequest request)
    {
        var result = await _writeService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ShippingCostDTO>> UpdateAsync(int id, UpdateShippingCostRequest request)
    {
        var result = await _writeService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        await _writeService.DeleteAsync(id);
        return NoContent();
    }
}