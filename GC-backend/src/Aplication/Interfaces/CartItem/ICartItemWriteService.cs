using Applications.dtos;

namespace Aplication.Interfaces.CartItem;

public interface ICartItemWriteService
{
    Task CreateCartAsync(int userId);
    Task<CartItemDTO> AddCartItemAsync(int userId, CreateCartItemRequest request);
    Task<CartItemDTO> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemRequest request);
    Task DeleteCartItemAsync(int userId, int cartItemId);
}