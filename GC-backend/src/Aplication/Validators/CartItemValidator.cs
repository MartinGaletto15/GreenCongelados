using Domain.Entities;
using Domain.Exceptions;

namespace Aplication.Validators;

public static class CartItemValidator
{
    public static void ValidateCartExists(Cart? cart)
    {
        if (cart == null)
            throw new AppValidationException("El usuario no tiene un carrito activo.", "CART_NOT_FOUND");
    }

    public static void ValidateCartItemOwnership(CartItem? cartItem, Cart cart)
    {
        if (cartItem == null)
            throw new AppValidationException("Item del carrito no encontrado.", "CART_ITEM_NOT_FOUND");

        if (cartItem.IdCart != cart.IdCart)
            throw new AppValidationException("No tienes permisos para realizar esta operación sobre este item.", "CART_ITEM_FORBIDDEN");
    }
}
