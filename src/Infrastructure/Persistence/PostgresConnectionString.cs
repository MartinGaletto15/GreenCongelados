using Npgsql;

namespace Infrastructure.Persistence;

/// <summary>
/// Normaliza la cadena de conexión de PostgreSQL. Supabase entrega una URI
/// (postgresql://...) y Npgsql espera el formato Host=...;Port=...
/// </summary>
public static class PostgresConnectionString
{
    public static string Normalize(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return connectionString;

        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
            throw new InvalidOperationException("La cadena de conexión de PostgreSQL no es una URI válida.");

        var credentials = uri.UserInfo.Split(':', 2);
        if (credentials.Length != 2 || string.IsNullOrWhiteSpace(credentials[0]))
            throw new InvalidOperationException("La URI de PostgreSQL debe incluir usuario y contraseña.");

        var database = uri.AbsolutePath.Trim('/');
        if (string.IsNullOrWhiteSpace(database))
            database = "postgres";

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = Uri.UnescapeDataString(database),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = Uri.UnescapeDataString(credentials[1]),
            // Supabase exige TLS.
            SslMode = SslMode.Require
        };

        return builder.ConnectionString;
    }
}
