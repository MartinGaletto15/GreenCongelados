using Application.Interfaces.Security;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Security;

/// <summary>
/// Valida los JWT contra la versión de seguridad persistida del usuario.
/// Al consultar la base de datos en cada petición autenticada, la revocación
/// es inmediata y sobrevive a reinicios y a múltiples instancias.
/// </summary>
public sealed class UserSessionValidator : IUserSessionValidator
{
    private readonly GCContext _context;

    public UserSessionValidator(GCContext context)
    {
        _context = context;
    }

    public async Task<bool> IsTokenValidAsync(int userId, string? securityStamp)
    {
        if (!Guid.TryParse(securityStamp, out var stamp))
            return false;

        var currentStamp = await _context.Users
            .AsNoTracking()
            .Where(user => user.IdUser == userId)
            .Select(user => (Guid?)user.SecurityStamp)
            .FirstOrDefaultAsync();

        // Sin fila el usuario fue eliminado; sin coincidencia la versión cambió.
        return currentStamp == stamp;
    }
}
