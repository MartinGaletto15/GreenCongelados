using Aplication.Interfaces.Category;
using Applications.dtos;
using Applications.dtos.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Categories;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllAsync()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDTO>> GetByIdAsync(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDTO>> CreateAsync(CreateCategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDTO>> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var result = await _categoryService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}