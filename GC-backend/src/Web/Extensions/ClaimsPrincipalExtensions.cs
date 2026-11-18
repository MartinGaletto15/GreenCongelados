using System.Security.Claims;
using Domain.Entities.Enums;
using Domain.Exceptions;

namespace Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(value, out var id))
            throw new AppValidationException("Token inválido.", "UNAUTHORIZED", 401);
        return id;
    }

    public static Role GetRole(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.Role);
        if (!Enum.TryParse<Role>(value, out var role))
            throw new AppValidationException("Token inválido.", "UNAUTHORIZED", 401);
        return role;
    }
}
