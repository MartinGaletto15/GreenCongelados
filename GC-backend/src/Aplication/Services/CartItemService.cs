using Aplication.Interfaces.CartItem;
using Applications.dtos;
using Aplication.DTOs.Requests.Create;
using Aplication.DTOs.Requests.Update;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;

namespace Aplication.Services;

public class CartItemService : ICartItemReadOnlyService, ICartItemWriteService
{
    private readonly ICartItemRepository _repository;

    public CartItemService(ICartItemRepository repository)
    {
        _repository = repository;
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
        
        // UX: Si el usuario no tiene carrito, lo creamos automáticamente
        if (cart == null)
        {
            cart = await _repository.CreateCartAsync(userId);
        }

        var cartItem = new CartItem
        {
            IdCart = cart.IdCart,
            IdProduct = request.IdProduct,
            Quantity = request.Quantity
        };

        await _repository.AddAsync(cartItem);
        return CartItemDTO.Create(cartItem);
    }

    public async Task<CartItemDTO> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemRequest request)
    {
        var cart = await _repository.GetUserCartAsync(userId);
        if (cart == null) 
            throw new AppValidationException("El usuario no tiene un carrito activo.", "CART_NOT_FOUND");

        var cartItem = await _repository.GetByIdAsync(cartItemId);
        if (cartItem == null) 
            throw new AppValidationException("Item del carrito no encontrado.", "CART_ITEM_NOT_FOUND");
        
        if (cartItem.IdCart != cart.IdCart)
            throw new AppValidationException("No tienes permisos para modificar este item.", "CART_ITEM_FORBIDDEN");

        cartItem.Quantity = request.Quantity;

        await _repository.UpdateAsync(cartItem);
        return CartItemDTO.Create(cartItem);
    }

    public async Task DeleteCartItemAsync(int userId, int cartItemId)
    {
        var cart = await _repository.GetUserCartAsync(userId);
        if (cart == null) 
            throw new AppValidationException("El usuario no tiene un carrito activo.", "CART_NOT_FOUND");

        var cartItem = await _repository.GetByIdAsync(cartItemId);
        if (cartItem == null) 
            throw new AppValidationException("Item del carrito no encontrado.", "CART_ITEM_NOT_FOUND");

        if (cartItem.IdCart != cart.IdCart) 
            throw new AppValidationException("No tienes permisos para eliminar este item.", "CART_ITEM_FORBIDDEN");

        await _repository.DeleteAsync(cartItem);
    }
}