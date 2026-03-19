using Applications.dtos;

namespace Aplication.Interfaces.CartItem;

public interface ICartItemReadOnlyService
{
    Task<IEnumerable<CartItemDTO>> GetCartItemsAsync(int userId);
}