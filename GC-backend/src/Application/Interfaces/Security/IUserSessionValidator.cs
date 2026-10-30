namespace Application.Interfaces.Security;

/// <summary>
/// Comprueba contra el almacén persistente si un JWT sigue siendo válido.
/// </summary>
public interface IUserSessionValidator
{
    /// <summary>
    /// Devuelve true solo si el usuario existe y su versión de seguridad
    /// coincide con la que se incluyó al emitir el token.
    /// </summary>
    Task<bool> IsTokenValidAsync(int userId, string? securityStamp);
}
