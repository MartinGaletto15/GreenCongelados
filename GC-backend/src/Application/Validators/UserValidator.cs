using Application.DTOs.Requests;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Validators;

public static class UserValidator
{

    public static void ValidateCompleteRegistration(CompleteRegistrationRequest request)
    {
        ValidatePassword(request.Password);
    }

    // Única definición de la política de contraseñas. La usan el registro y el reset.
    public static void ValidatePassword(string password)
    {
        if (!Validations.ValidatePassword(password, 6, 20, true, true))
            throw new AppValidationException("La contraseña debe tener entre 6 y 20 caracteres e incluir al menos una mayúscula y un número.", "USER_PASSWORD_INVALID");
    }

    public static void ValidateRoleUpdate(Role newRole, Role currentTargetRole, Role performerRole)
    {
        if (!Enum.IsDefined(typeof(Role), newRole))
            throw new AppValidationException("El rol indicado no es válido.", "USER_ROLE_INVALID");

        // Performer must have a higher hierarchy (lower numerical value) than the target user's current role
        if (performerRole >= currentTargetRole)
            throw new AppValidationException("No tienes permisos para modificar el rol de este usuario.", "USER_ROLE_INSUFFICIENT_PERMISSIONS", 403);

        // Performer can only assign roles with a lower hierarchy than their own
        if (performerRole >= newRole)
            throw new AppValidationException("No tienes permisos para asignar este rol.", "USER_ROLE_INVALID_ASSIGNMENT", 403);
    }
}
