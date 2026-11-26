using Application.Interfaces.CartItem;
using Application.DTOs;
using Application.DTOs.Requests.Create;
using Application.DTOs.Requests.Update;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using Domain.Exceptions;
using Application.Validators;

namespace Application.Services;

public class CartItemService : ICartItemReadOnlyService, ICartItemWriteService
{
    private readonly ICartItemRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CartItemService(ICartItemRepository repository, IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    // Gets all cart items for a specific user.
    public async Task<IEnumerable<CartItemDTO>> GetCartItemsAsync(int userId)
    {
        var entities = await _repository.GetByUserIdAsync(userId);
        return CartItemDTO.Create(entities);
    }

    public async Task CreateCartAsync(int userId)
    {
        var cart = await _repository.GetUserCartAsync(userId);
        if (cart == null)
        {
            await _repository.CreateCartAsync(userId);
        }
    }

    public async Task<CartItemDTO> AddCartItemAsync(int userId, CreateCartItemRequest request)
    {
        var cart = await _repository.GetUserCartAsync(userId);
        
        // UX: If user doesn't have a cart, we create it automatically
        if (cart == null)
        {
            cart = await _repository.CreateCartAsync(userId);
        }

        var product = await _productRepository.GetByIdAsync(request.IdProduct);
        if (product == null || product.Status == ProductStatus.Deleted)
            throw new AppValidationException("Producto no encontrado.", "PRODUCT_NOT_FOUND", 404);

        // Se busca en el carrito ya trackeado: reutilizar la instancia evita
        // adjuntar una copia con la misma clave y romper el ChangeTracker.
        var existingItem = cart.CartItems.FirstOrDefault(ci => ci.IdProduct == request.IdProduct);

        // Acumulativo: lo que ya está en el carrito + lo que se agrega ahora.
        int totalQuantity = request.Quantity + (existingItem?.Quantity ?? 0);

        // Cantidad total, disponibilidad y stock: una sola regla de dominio.
        EnsureTotalStock(product, totalQuantity, existingItem?.Quantity ?? 0);

        if (existingItem != null)
        {
            existingItem.Quantity = totalQuantity;
            await _repository.UpdateAsync(existingItem);
            await _unitOfWork.SaveChangesAsync();
            return CartItemDTO.Create(existingItem);
        }

        var cartItem = new CartItem
        {
            IdCart = cart.IdCart,
            IdProduct = request.IdProduct,
            Quantity = request.Quantity
        };

        await _repository.AddAsync(cartItem);
        await _unitOfWork.SaveChangesAsync();
        return CartItemDTO.Create(cartItem);
    }

    public async Task<CartItemDTO> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemRequest request)
    {
        var cart = await GetUserCartOrThrowAsync(userId);
        var cartItem = GetCartItemOrThrow(cart, cartItemId);

        return await SetQuantityAsync(cartItem, request.Quantity);
    }

    public async Task DeleteCartItemAsync(int userId, int cartItemId)
    {
        var cart = await GetUserCartOrThrowAsync(userId);
        var cartItem = GetCartItemOrThrow(cart, cartItemId);

        await _repository.DeleteAsync(cartItem);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ClearCartAsync(int userId)
    {
        // Una sola sentencia: ExecuteDelete corre en la misma conexión y transacción
        // del contexto, así que participa del BeginTransaction de la orden.
        await _repository.DeleteByUserIdAsync(userId);
    }

    // El carrito se busca siempre por el usuario del token: el cliente nunca
    // decide de quién es el carrito.
    private async Task<Cart> GetUserCartOrThrowAsync(int userId)
    {
        var cart = await _repository.GetUserCartAsync(userId);
        CartItemValidator.ValidateCartExists(cart);
        return cart!;
    }

    // Se busca dentro de los items del carrito del usuario, no en toda la tabla:
    // un item de otro carrito simplemente no existe para esta consulta.
    private static CartItem GetCartItemOrThrow(Cart cart, int cartItemId)
    {
        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.IdCartItem == cartItemId);
        CartItemValidator.ValidateCartItemExists(cartItem);
        return cartItem!;
    }

    private async Task<CartItemDTO> SetQuantityAsync(CartItem cartItem, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(cartItem.IdProduct);
        if (product == null) throw new AppValidationException("Producto no encontrado.", "PRODUCT_NOT_FOUND", 404);

        product.EnsureAvailableFor(quantity);

        cartItem.Quantity = quantity;

        await _repository.UpdateAsync(cartItem);
        await _unitOfWork.SaveChangesAsync();
        return CartItemDTO.Create(cartItem);
    }

    // La regla de stock vive en Product; acá solo se enriquece el mensaje para
    // que se entienda por qué una cantidad igual al stock no cabe en el carrito.
    private static void EnsureTotalStock(Product product, int totalQuantity, int alreadyInCart)
    {
        try
        {
            product.EnsureAvailableFor(totalQuantity);
        }
        catch (AppValidationException ex) when (alreadyInCart > 0 && ex.ErrorCode == "INSUFFICIENT_STOCK")
        {
            throw new AppValidationException(
                $"Stock insuficiente para el producto: {product.Name}. Ya tienes {alreadyInCart} en el carrito y el total solicitado ({totalQuantity}) supera el disponible ({product.CurrentStock}).",
                ex.ErrorCode, ex.StatusCode);
        }
    }
}