using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Enums;
using Domain.Exceptions;

namespace Domain.Entities;
public class User
{
    [Key]
    public int IdUser { get; private set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; private set; }

    [Required]
    [MaxLength(50)]
    public string LastName { get; private set; }

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; private set; }

    [Required]
    [MaxLength(100)]
    public string Password { get; private set; }
    
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

    // EF constructor
    protected User() {
        Name = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
    }

    public User(string name, string lastName, string email, string passwordHash, Role role = Role.USER)
    {
        ValidateData(name, lastName, email);
        
        Name = name;
        LastName = lastName;
        Email = email;
        Password = passwordHash;
        Role = role;
    }

    public void UpdateDetails(string? name, string? lastName, string? phone)
    {
        if (name != null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new AppValidationException("Name cannot be empty", "INVALID_NAME");
            Name = name;
        }

        if (lastName != null)
        {
            if (string.IsNullOrWhiteSpace(lastName)) throw new AppValidationException("Last name cannot be empty", "INVALID_LASTNAME");
            LastName = lastName;
        }

        Phone = phone ?? Phone;
    }

    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new AppValidationException("Password cannot be empty", "INVALID_PASSWORD");
        Password = passwordHash;
    }

    public void ChangeRole(Role newRole)
    {
        Role = newRole;
    }

    private void ValidateData(string name, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new AppValidationException("Name cannot be empty", "INVALID_NAME");
        if (string.IsNullOrWhiteSpace(lastName)) throw new AppValidationException("Last name cannot be empty", "INVALID_LASTNAME");
        if (string.IsNullOrWhiteSpace(email)) throw new AppValidationException("Email cannot be empty", "INVALID_EMAIL");
    }
}
