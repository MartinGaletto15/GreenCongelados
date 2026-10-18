using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Domain.Entities;
public class PasswordResetToken {
    [Key]
    [Required]
    [NotNull]
    public int IdPasswordResetToken { get; private set; }

    [Required]
    [NotNull]
    public int IdUser { get; private set; }

    [Required]
    [NotNull]
    public int Token { get; private set; }

    [Required]
    [NotNull]
    public DateTime ExpiresAt { get; private set; }

    [Required]
    [NotNull]
    public DateTime CreateAt { get; private set; }

    [NotNull]
    public bool Used { get; private set; }

    protected PasswordResetToken() {}

    public PasswordResetToken(int idUser, int token, int expirationMinutes = 15)
    {
        if (idUser <= 0) 
            throw new ArgumentException("IdUser inválido", nameof(idUser));

        IdUser = idUser;
        Token = token;
        CreateAt = DateTime.UtcNow;
        ExpiresAt = CreateAt.AddMinutes(expirationMinutes);
        Used = false;
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

    public void MarkAsUsed()
    {
        Used = true;
    }
}