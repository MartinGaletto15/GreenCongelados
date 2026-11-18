using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Domain.Entities.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class User
{
    [Key]
    public int IdUser { get; private set; }

    [MaxLength(50)]
    public string? Name { get; private set; }

    [MaxLength(50)]
    public string? LastName { get; private set; }

    [Required]
    [MaxLength(100)]
    public string Email { get; private set; }

    [Required]
    public bool EmailConfirmed { get; private set; }

    [MaxLength(100)]
    public string? Password { get; private set; }

    [MaxLength(25)]
    public string? Phone { get; private set; }

    public Role Role { get; private set; } = Role.USER;

    public Guid SecurityStamp { get; private set; } = Guid.NewGuid();

    // Orders the user PLACED (as Client)
    [InverseProperty("User")]
    public ICollection<Order> OrdersPlaced { get; private set; } = new List<Order>();

    // Orders the user DELIVERS (as Courier)
    [InverseProperty("Courier")]
    public ICollection<Order> OrdersToDeliver { get; private set; } = new List<Order>();

    // Cart belonging to the user
    public Cart? Cart { get; private set; }

    public bool IsActive { get; private set; }

    public ICollection<UserToken> UserTokens { get; private set; } = new List<UserToken>();

    protected User()
    {
        Name = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
    }

    public User(string email, Role role = Role.USER)
    {
        string validateEmail = ValidateEmail(email);

        Email = validateEmail;
        Role = role;
    }

    // Actualización parcial: un valor null deja el campo como está.
    public void UpdateDetails(string? name, string? lastName, string? phone = null)
    {
        if (name is not null)
            Name = ValidateName(name);

        if (lastName is not null)
            LastName = ValidateName(lastName);

        // Un teléfono vacío borra el dato; null significa "no cambiar".
        if (phone is not null)
            Phone = ValidatePhone(phone);
    }

    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new AppValidationException("La contraseña no puede estar vacía.", "INVALID_PASSWORD");
        Password = passwordHash;

        // Cambiar la contraseña cierra las sesiones abiertas con la anterior.
        RotateSecurityStamp();
    }

    public void CompleteRegistration(string name, string lastName, string passwordHash)
    {
        if (EmailConfirmed && IsActive)
        {
            throw new AppValidationException("Tu cuenta ya está completa. Inicia sesión.", "USER_REGISTRATION_ALREADY_COMPLETED", 409);
        }

        // El registro no es una actualización parcial: nombre y apellido son
        // obligatorios, así que se validan siempre (un null no se ignora).
        Name = ValidateName(name);
        LastName = ValidateName(lastName);
        UpdatePassword(passwordHash);
        SetActiveAccount();

        // La cuenta recién queda confirmada cuando el usuario completa el registro.
        EmailConfirm();
    }

    internal void EmailConfirm()
    {
        EmailConfirmed = true;
    }

    public void ChangeRole(Role newRole)
    {
        if (Role == newRole) return;

        Role = newRole;

        // Un token emitido con el rol anterior no debe conservar sus permisos.
        RotateSecurityStamp();
    }

    /// Genera una nueva versión de seguridad. Los JWT emitidos antes dejan de
    /// ser aceptados en la siguiente validación.
    public void RotateSecurityStamp()
    {
        SecurityStamp = Guid.NewGuid();
    }

    public void SetInactiveAccount()
    {
        // La cuenta vuelve al estado "sin completar": no puede iniciar sesión ni
        // restablecer contraseña, y solo se reactiva repitiendo el registro con
        // un código de confirmación.
        IsActive = false;
        EmailConfirmed = false;
        RotateSecurityStamp();
    }

    internal void SetActiveAccount()
    {
        IsActive = true;
    }

    // El formato del email se valida en el DTO ([EmailAddress]). Aquí solo se
    // garantiza el invariante: nunca vacío y siempre normalizado.
    private static string ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new AppValidationException("Ingresa un email válido.", "USER_EMAIL_INVALID");

        return NormalizeEmail(email);
    }

    private static string ValidateName(string? str)
    {
        if (!ValidateString(str, 2, 50))
        {
            throw new AppValidationException("El nombre y apellido son obligatorios y deben tener entre 2 y 50 caracteres.", "USER_NAME_INVALID");
        }

        string validName = NormalizeName(str);
        return validName;
    }

    // Devuelve null cuando el valor indica borrar el teléfono. Solo admite dígitos.
    private static string? ValidatePhone(string phone)
    {
        if (phone.Length == 0)
            return null;

        if (!phone.All(char.IsAsciiDigit))
            throw new AppValidationException("El teléfono solo puede contener dígitos.", "USER_PHONE_INVALID");

        if (phone.Length < 7 || phone.Length > 25)
            throw new AppValidationException("El teléfono debe tener entre 7 y 25 dígitos.", "USER_PHONE_INVALID");

        return phone;
    }

    private static bool ValidateString([NotNullWhen(true)] string? str, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(str))
            return false;

        // Se mide el valor ya normalizado: "  a  " no debe contar como nombre válido.
        string value = str.Trim();

        if (minLength > 0 && value.Length < minLength)
            return false;

        if (maxLength > 0 && value.Length > maxLength)
            return false;

        return true;
    }

    // normalizaciones
    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    public static string NormalizeName(string str)
    {
        string normalizedName = str.Trim();
        normalizedName = char.ToUpper(normalizedName[0]) + normalizedName[1..];
        return normalizedName;
    }
}