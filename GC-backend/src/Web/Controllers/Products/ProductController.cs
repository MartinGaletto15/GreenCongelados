using Aplication.Interfaces.Product;
using Applications.dtos;
using Applications.dtos.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers.Products;

[Authorize(Roles = "Admin,SuperAdmin")]
[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductWriteService _writeProductService;
    private readonly IProductReadOnlyService _readProductService;

    public ProductController(IProductWriteService writeProductService, IProductReadOnlyService readProductService)
    {
        _writeProductService = writeProductService;
        _readProductService = readProductService;
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
}