using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Aplication.Interfaces.Promotion;
using Applications.dtos;
using Applications.dtos.Requests;

namespace Web.Controllers.Promotion;

[Authorize(Roles = "Admin,SuperAdmin")]
[ApiController]
[Route("api/promotions")]
public class PromotionController : ControllerBase
{
    private readonly IPromotionWriteService _writePromotionService;
    private readonly IPromotionReadOnlyService _readPromotionService;

    public PromotionController(IPromotionWriteService writePromotionService, IPromotionReadOnlyService readPromotionService)
    {
        _writePromotionService = writePromotionService;
        _readPromotionService = readPromotionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PromotionDTO>>> GetAllAsync()
    {
        var result = await _readPromotionService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PromotionDTO>> GetByIdAsync(int id)
    {
        var result = await _readPromotionService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PromotionDTO>> CreateAsync(CreatePromotionRequest request)
    {
        var result = await _writePromotionService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PromotionDTO>> UpdateAsync(int id, UpdatePromotionRequest request)
    {
        var result = await _writePromotionService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        await _writePromotionService.DeleteAsync(id);
        return NoContent();
    }
}