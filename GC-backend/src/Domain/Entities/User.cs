using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    [EmailAddress]
    public string Email { get; private set; }

    [Required]
    public bool EmailConfirmed { get; private set; }

    [MaxLength(100)]
    public string? Password { get; private set; }

    [MaxLength(25)]
    public string? Phone { get; private set; }

    public Role Role { get; private set; } = Role.USER;


    // Orders the user PLACED (as Client)
    [InverseProperty("User")]
    public ICollection<Order> OrdersPlaced { get; private set; } = new List<Order>();

    // Orders the user DELIVERS (as Courier)
    [InverseProperty("Courier")]
    public ICollection<Order> OrdersToDeliver { get; private set; } = new List<Order>();

    // Cart belonging to the user
    public Cart? Cart { get; private set; }

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
        if (string.IsNullOrWhiteSpace(email)) throw new AppValidationException("Email cannot be empty", "INVALID_EMAIL");

        Email = email;
        Role = role;
    }

    public void UpdateDetails(string? name, string? lastName, string? phone = null)
    {
        if (name != null && string.IsNullOrWhiteSpace(name))
            throw new AppValidationException("Name cannot be empty", "INVALID_NAME");

        if (lastName != null && string.IsNullOrWhiteSpace(lastName))
            throw new AppValidationException("Last name cannot be empty", "INVALID_LASTNAME");

        if (name != null) Name = name;
        if (lastName != null) LastName = lastName;
        Phone = phone ?? Phone;
    }

    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new AppValidationException("Password cannot be empty", "INVALID_PASSWORD");
        Password = passwordHash;
    }

    public void CompleteRegistration(string name, string lastName, string passwordHash)
    {
        if (Password is not null)
        {
            throw new AppValidationException("Tu cuenta ya está completa. Inicia sesión.", "USER_REGISTRATION_ALREADY_COMPLETED");
        }

        UpdateDetails(name, lastName);
        UpdatePassword(passwordHash);

        // La cuenta recién queda confirmada cuando el usuario completa el registro.
        EmailConfirm();
    }

    public void EmailConfirm()
    {
        EmailConfirmed = true;
    }

    public void ChangeRole(Role newRole)
    {
        Role = newRole;
    }
}