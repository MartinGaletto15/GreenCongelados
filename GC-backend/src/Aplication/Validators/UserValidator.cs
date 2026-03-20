using Applications.dtos.Requests;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Aplication.Validators;

public static class UserValidator
{
    public static async Task ValidateCreateAsync(CreateUserRequest request, IUserRepository repository)
    {
        if (!Validations.ValidateEmail(request.Email))
            throw new AppValidationException("El email provisto no es válido.", "USER_EMAIL_INVALID");

        if (!Validations.ValidatePassword(request.Password, 6, 20, true, true))
            throw new AppValidationException("La contraseña debe tener entre 6 y 20 caracteres, e incluir al menos una mayúscula y un número.", "USER_PASSWORD_INVALID");

        if (!Validations.ValidateString(request.Name, 2, 50))
            throw new AppValidationException("El nombre es obligatorio y debe tener entre 2 y 50 caracteres.", "USER_NAME_INVALID");

        if (!Validations.ValidateString(request.LastName, 2, 50))
            throw new AppValidationException("El apellido es obligatorio y debe tener entre 2 y 50 caracteres.", "USER_LASTNAME_INVALID");

        var existingUser = await repository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new AppValidationException("El email ya existe en la base de datos.", "USER_EMAIL_EXISTS");
    }

    public static async Task ValidateUpdateAsync(int id, UpdateUserRequest request, IUserRepository repository, User currentUser)
    {
        if (request.Email != null && !Validations.ValidateEmail(request.Email))
            throw new AppValidationException("El email de referencia provisto no es válido.", "USER_EMAIL_INVALID");

        if (!string.IsNullOrEmpty(request.Password) && !Validations.ValidatePassword(request.Password, 6, 20, true, true))
            throw new AppValidationException("La contraseña debe tener entre 6 y 20 caracteres, e incluir al menos una mayúscula y un número.", "USER_PASSWORD_INVALID");

        if (!string.IsNullOrEmpty(request.Name) && !Validations.ValidateString(request.Name, 2, 50))
            throw new AppValidationException("El nombre debe tener entre 2 y 50 caracteres.", "USER_NAME_INVALID");

        if (!string.IsNullOrEmpty(request.LastName) && !Validations.ValidateString(request.LastName, 2, 50))
            throw new AppValidationException("El apellido debe tener entre 2 y 50 caracteres.", "USER_LASTNAME_INVALID");

        if (request.Email != null && currentUser.Email != request.Email)
        {
            var existingUser = await repository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new AppValidationException("El email ya existe en la base de datos.", "USER_EMAIL_EXISTS");
        }
    }

    public static void ValidateRoleUpdate(Role newRole, Role currentTargetRole, Role performerRole)
    {
        // El ejecutor debe tener una jerarquía superior (valor numérico menor) al rol actual del usuario
        if (performerRole >= currentTargetRole)
            throw new AppValidationException("No tienes permisos para modificar el rol de este usuario.", "USER_ROLE_INSUFFICIENT_PERMISSIONS");

        // El ejecutor solo puede asignar roles de jerarquía inferior a la suya
        if (performerRole >= newRole)
            throw new AppValidationException("No tienes permisos para asignar este rol.", "USER_ROLE_INVALID_ASSIGNMENT");
    }
}
