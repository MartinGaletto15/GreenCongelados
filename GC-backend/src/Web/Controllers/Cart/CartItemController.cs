namespace Web.Controllers.Cart;

[ApiController]
[Route("api/cart/items")]
[Authorize]
public class CartItemController : ControllerBase
{
    private readonly ICartItemWriteService _cartItemWriteService;
    private readonly ICartItemReadOnlyService _cartItemReadOnlyService;

    public CartItemController(ICartItemWriteService cartItemWriteService, ICartItemReadOnlyService cartItemReadOnlyService)
    {
        _cartItemWriteService = cartItemWriteService;
        _cartItemReadOnlyService = cartItemReadOnlyService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartItemDTO>>> GetCartItemsAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cartItems = await _cartItemReadOnlyService.GetCartItemsAsync(userId);
        return Ok(cartItems);
    }

    [HttpPost]
    public async Task<ActionResult<CartItemDTO>> AddCartItemAsync(CreateCartItemRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cartItem = await _cartItemWriteService.AddCartItemAsync(userId, request);
        return Ok(cartItem);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CartItemDTO>> UpdateCartItemAsync(int id, UpdateCartItemRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cartItem = await _cartItemWriteService.UpdateCartItemAsync(userId, id, request);
        return Ok(cartItem);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCartItemAsync(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _cartItemWriteService.DeleteCartItemAsync(userId, id);
        return NoContent();
    }
}