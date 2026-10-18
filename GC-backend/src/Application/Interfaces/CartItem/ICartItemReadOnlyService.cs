using Application.DTOs;

namespace Application.Interfaces.CartItem;

public interface ICartItemReadOnlyService
{
    Task<IEnumerable<CartItemDTO>> GetCartItemsAsync(int userId);
}