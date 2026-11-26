using Domain.Entities;
using Domain.Exceptions;

namespace Application.Validators;

public static class CartItemValidator
{
    public static void ValidateCartExists(Cart? cart)
    {
        if (cart == null)
            throw new AppValidationException("El usuario no tiene un carrito activo.", "CART_NOT_FOUND", 404);
    }

    // El item se busca siempre dentro del carrito del usuario, así que si no
    // aparece es porque no existe o es de otro carrito: mismo 404 en ambos casos.
    public static void ValidateCartItemExists(CartItem? cartItem)
    {
        if (cartItem == null)
            throw new AppValidationException("Item del carrito no encontrado.", "CART_ITEM_NOT_FOUND", 404);
    }
}
