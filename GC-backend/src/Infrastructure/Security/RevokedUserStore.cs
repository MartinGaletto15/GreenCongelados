using System.Collections.Concurrent;
using Application.Interfaces.Security;

namespace Infrastructure.Security;

/// <summary>
/// Mantiene en memoria los usuarios eliminados para rechazar sus JWT hasta que
/// venzan. Evita consultar la base de datos en cada petición autenticada.
/// </summary>
public sealed class RevokedUserStore : IRevokedUserStore
{
    // Debe coincidir con la vigencia del JWT configurada en JwtProvider.

    private readonly ConcurrentDictionary<int, DateTime> _revokedUsers = new();

    public void Revoke(int userId)
    {
        var now = DateTime.UtcNow;
        _revokedUsers[userId] = now.AddHours(1);

        // Limpieza oportunista: las entradas vencidas ya no aportan nada.
        foreach (var (id, expiresAt) in _revokedUsers)
        {
            if (expiresAt <= now)
            {
                _revokedUsers.TryRemove(id, out _);
            }
        }
    }

    public bool IsRevoked(int userId)
    {
        return _revokedUsers.TryGetValue(userId, out var expiresAt) && expiresAt > DateTime.UtcNow;
    }
}
