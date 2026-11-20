using Application.DTOs.Requests;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Validators;

public static class UserValidator
{

    // Registro: un email sin confirmar se reutiliza (se reenvía el token), pero
    // una cuenta ya confirmada y activa no puede volver a registrarse.
    public static void ValidateRegistration(User? existingUser)
    {
        if (existingUser is not null && existingUser.EmailConfirmed && existingUser.IsActive)
            throw new AppValidationException("Ya existe una cuenta con este email. Inicia sesión o restablece tu contraseña.", "USER_ALREADY_EXISTS", 409);
    }

    // Login: mismo mensaje para "no existe" y "está inactivo" para no filtrar cuentas.
    public static User ValidateLogin(User? user)
    {
        if (user is null || !user.IsActive)
            throw new AppValidationException("Email o contraseña incorrectos.", "AUTH_INVALID_CREDENTIALS");

        if (!user.EmailConfirmed || user.Password is null)
            throw new AppValidationException("Tu cuenta todavía no está confirmada. Revisa tu correo y completa el registro.", "USER_EMAIL_NOT_CONFIRMED");

        return user;
    }

    public static void ValidateCompleteRegistration(CompleteRegistrationRequest request)
    {
        ValidatePassword(request.Password);
    }

    // Única definición de la política de contraseñas. La usan el registro y el reset.
    public static void ValidatePassword(string password)
    {
        bool isValid = !string.IsNullOrEmpty(password)
            && password.Length >= 6
            && password.Length <= 20
            && password.Any(char.IsUpper)
            && password.Any(char.IsDigit);

        if (!isValid)
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
