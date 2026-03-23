using Aplication.Interfaces.Product;
using Aplication.Interfaces.ProductCategory;
using Applications.dtos;
using Applications.dtos.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers.Products;

[Authorize(Roles = "ADMIN,SUPERADMIN")]
[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductWriteService _writeProductService;
    private readonly IProductReadOnlyService _readProductService;
    private readonly IProductCategoryReadOnlyService _readProductCategoryService;
    private readonly IProductCategoryWriteService _writeProductCategoryService;

    public ProductController(
        IProductWriteService writeProductService,
        IProductReadOnlyService readProductService,
        IProductCategoryReadOnlyService readProductCategoryService,
        IProductCategoryWriteService writeProductCategoryService)
    {
        _writeProductService = writeProductService;
        _readProductService = readProductService;
        _readProductCategoryService = readProductCategoryService;
        _writeProductCategoryService = writeProductCategoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAllAsync()
    {
        var result = await _readProductService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDTO>> GetByIdAsync(int id)
    {
        var result = await _readProductService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDTO>> CreateAsync(CreateProductRequest request)
    {
        var result = await _writeProductService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDTO>> UpdateAsync(int id, UpdateProductRequest request)
    {
        var result = await _writeProductService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        await _writeProductService.DeleteAsync(id);
        return NoContent();
    }

    // PRODUCT CATEGORY NESTED ROUTES

    [HttpGet("{id}/categories")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategoriesAsync(int id)
    {
        var result = await _readProductCategoryService.GetCategoriesByProductIdAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/categories")]
    public async Task<ActionResult> AddCategoryAsync(int id, ProductAssociateCategoryRequest request)
    {
        await _writeProductCategoryService.AddCategoryToProductAsync(id, request.IdCategory);
        return NoContent();
    }

    [HttpDelete("{id}/categories/{idCategory}")]
    public async Task<ActionResult> RemoveCategoryAsync(int id, int idCategory)
    {
        await _writeProductCategoryService.RemoveCategoryFromProductAsync(id, idCategory);
        return NoContent();
    }

    [HttpPut("{id}/categories")]
    public async Task<ActionResult> SyncCategoriesAsync(int id, [FromBody] IEnumerable<int> categoryIds)
    {
        await _writeProductCategoryService.SyncCategoriesToProductAsync(id, categoryIds);
        return NoContent();
    }
}