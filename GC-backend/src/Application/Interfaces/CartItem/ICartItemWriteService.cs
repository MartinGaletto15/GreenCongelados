using Application.DTOs;
using Application.DTOs.Requests.Create;
using Application.DTOs.Requests.Update;

namespace Application.Interfaces.CartItem;

public interface ICartItemWriteService
{
    Task CreateCartAsync(int userId);
    Task<CartItemDTO> AddCartItemAsync(int userId, CreateCartItemRequest request);
    Task<CartItemDTO> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemRequest request);
    Task<CartItemDTO> UpdateCartItemByProductAsync(int userId, int productId, UpdateCartItemRequest request);
    Task DeleteCartItemAsync(int userId, int cartItemId);
    Task ClearCartAsync(int userId);
}