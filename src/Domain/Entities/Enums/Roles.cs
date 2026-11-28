namespace Domain.Entities.Enums;

/// Nombres de rol tal como se emiten en el claim (Role.ToString())
/// y listas listas para usar en [Authorize(Roles = ...)].
public static class Roles
{
    public const string SuperAdmin = nameof(Role.SUPERADMIN);
    public const string Admin = nameof(Role.ADMIN);
    public const string User = nameof(Role.USER);
    public const string Cadet = nameof(Role.CADET);

    public const string AdminOrSuperAdmin = Admin + "," + SuperAdmin;
    public const string AdminSuperAdminOrCadet = AdminOrSuperAdmin + "," + Cadet;

    public static bool IsAdmin(string? role) => role is Admin or SuperAdmin;

    public static bool IsAdmin(Role role) => role is Role.ADMIN or Role.SUPERADMIN;
}
