namespace Application.Interfaces.Security;

/// <summary>
/// Claims propios que no forman parte de los estándares de JWT/ASP.NET.
/// </summary>
public static class JwtClaimNames
{
    /// <summary>
    /// Versión de seguridad del usuario al momento de emitir el token.
    /// </summary>
    public const string SecurityStamp = "security_stamp";
}
