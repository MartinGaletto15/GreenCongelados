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
    private readonly IUnitOfWork _unitOfWork;

    public CartItemService(ICartItemRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
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
        await _unitOfWork.SaveChangesAsync();
        return CartItemDTO.Create(cartItem);
    }

    public async Task<CartItemDTO> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemRequest request)
    {
        var cart = await _repository.GetUserCartAsync(userId);
        CartItemValidator.ValidateCartExists(cart);

        var cartItem = await _repository.GetByIdAsync(cartItemId);
        CartItemValidator.ValidateCartItemOwnership(cartItem, cart!);

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