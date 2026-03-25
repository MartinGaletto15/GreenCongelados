using Aplication.Interfaces.CartItem;
using Applications.dtos;
using Aplication.DTOs.Requests.Create;
using Aplication.DTOs.Requests.Update;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;
using Aplication.Validators;

namespace Aplication.Services;

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
        if (product == null) throw new AppValidationException("Producto no encontrado", "PRODUCT_NOT_FOUND");

        // Check if product is already in the cart
        var cartItems = await _repository.GetByUserIdAsync(userId);
        var existingItem = cartItems.FirstOrDefault(ci => ci.IdProduct == request.IdProduct);

        int totalQuantity = request.Quantity + (existingItem?.Quantity ?? 0);

        if (product.CurrentStock < totalQuantity)
        {
            throw new AppValidationException($"Stock insuficiente para el producto: {product.Name}. Disponible: {product.CurrentStock}", "INSUFFICIENT_STOCK");
        }

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
        var cart = await _repository.GetUserCartAsync(userId);
        CartItemValidator.ValidateCartExists(cart);

        var cartItem = await _repository.GetByIdAsync(cartItemId);
        CartItemValidator.ValidateCartItemOwnership(cartItem, cart!);

        var product = await _productRepository.GetByIdAsync(cartItem!.IdProduct);
        if (product == null) throw new AppValidationException("Producto no encontrado", "PRODUCT_NOT_FOUND");

        if (product.CurrentStock < request.Quantity)
        {
            throw new AppValidationException($"Stock insuficiente para el producto: {product.Name}. Disponible: {product.CurrentStock}", "INSUFFICIENT_STOCK");
        }

        cartItem!.Quantity = request.Quantity;

        await _repository.UpdateAsync(cartItem);
        await _unitOfWork.SaveChangesAsync();
        return CartItemDTO.Create(cartItem);
    }

    public async Task DeleteCartItemAsync(int userId, int cartItemId)
    {
        var cart = await _repository.GetUserCartAsync(userId);
        CartItemValidator.ValidateCartExists(cart);

        var cartItem = await _repository.GetByIdAsync(cartItemId);
        CartItemValidator.ValidateCartItemOwnership(cartItem, cart!);

        await _repository.DeleteAsync(cartItem!);
        await _unitOfWork.SaveChangesAsync();
    }
}