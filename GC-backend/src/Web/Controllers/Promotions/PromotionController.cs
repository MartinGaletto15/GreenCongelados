using Aplication.Interfaces.Promotion;
using Applications.dtos;
using Applications.dtos.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Promotions;

[ApiController]
[Route("api/promotions")]
public class PromotionController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PromotionDTO>>> GetAllAsync()
    {
        var result = await _promotionService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PromotionDTO>> GetByIdAsync(int id)
    {
        var result = await _promotionService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PromotionDTO>> CreateAsync(CreatePromotionRequest request)
    {
        var result = await _promotionService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PromotionDTO>> UpdateAsync(int id, UpdatePromotionRequest request)
    {
        var result = await _promotionService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        await _promotionService.DeleteAsync(id);
        return NoContent();
    }
}