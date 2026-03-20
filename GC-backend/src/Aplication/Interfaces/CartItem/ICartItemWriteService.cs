using Applications.dtos;
using Aplication.DTOs.Requests.Create;
using Aplication.DTOs.Requests.Update;

namespace Aplication.Interfaces.CartItem;

public interface ICartItemWriteService
{
    Task CreateCartAsync(int userId);
    Task<CartItemDTO> AddCartItemAsync(int userId, CreateCartItemRequest request);
    Task<CartItemDTO> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemRequest request);
    Task DeleteCartItemAsync(int userId, int cartItemId);
}