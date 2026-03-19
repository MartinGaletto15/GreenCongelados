using Aplication.Interfaces.Product;
using Applications.dtos;
using Applications.dtos.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Products;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAllAsync()
    {
        var result = await _productService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDTO>> GetByIdAsync(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDTO>> CreateAsync(CreateProductRequest request)
    {
        var result = await _productService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDTO>> UpdateAsync(int id, UpdateProductRequest request)
    {
        var result = await _productService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}