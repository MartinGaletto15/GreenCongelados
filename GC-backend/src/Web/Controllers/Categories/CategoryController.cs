using Aplication.Interfaces.Categories;
using Applications.dtos;
using Applications.dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Categories;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryReadOnlyService _readOnlyService;
    private readonly ICategoryWriteService _writeService;

    public CategoryController(ICategoryReadOnlyService readOnlyService, ICategoryWriteService writeService)
    {
        _readOnlyService = readOnlyService;
        _writeService = writeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllAsync()
    {
        var result = await _readOnlyService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDTO>> GetByIdAsync(int id)
    {
        var result = await _readOnlyService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<ActionResult<CategoryDTO>> CreateAsync(CreateCategoryRequest request)
    {
        var result = await _writeService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<ActionResult<CategoryDTO>> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var result = await _writeService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        await _writeService.DeleteAsync(id);
        return NoContent();
    }
}