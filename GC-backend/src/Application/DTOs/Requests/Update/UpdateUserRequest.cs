namespace Application.DTOs.Requests;

// Actualización parcial del perfil: cada campo null se deja sin cambios.
// En Phone, una cadena vacía borra el teléfono; si se envía un valor, debe
// contener solo dígitos (entre 7 y 25).
// La contraseña no se actualiza por este endpoint: el cambio solo se permite
// mediante el flujo verificado con código (PasswordResetService).
// Las reglas (largo de nombre, formato de teléfono, normalización) viven en
// User.UpdateDetails / User.ValidatePhone.
public record UpdateUserRequest(
    string? Name,
    string? LastName,
    string? Phone
);
