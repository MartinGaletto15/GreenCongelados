using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Domain.Entities.Enums;

namespace Domain.Entities;
public class UserToken {
    [Key]
    [Required]
    [NotNull]
    public int IdUserToken { get; private set; }

    [Required]
    [NotNull]
    public int IdUser { get; private set; }

    [Required]
    [NotNull]
    public string Token { get; private set; }

    [Required]
    [NotNull]
    public DateTime ExpiresAt { get; private set; }

    [Required]
    [NotNull]
    public DateTime CreateAt { get; private set; }

    [Required]
    public TokenType Type { get; private set; }

    [NotNull]
    public bool IsUsed { get; private set; }

    protected UserToken()
    {
        Token = string.Empty;
    }

    public UserToken(int idUser, string token, int expirationMinutes, TokenType type)
    {
        if (idUser <= 0) 
            throw new ArgumentException("IdUser inválido", nameof(idUser));
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("El token no puede estar vacío", nameof(token));
        if (expirationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(expirationMinutes), "La expiración debe ser mayor que cero.");

        IdUser = idUser;
        Token = token;
        CreateAt = DateTime.UtcNow;
        ExpiresAt = CreateAt.AddMinutes(expirationMinutes);
        Type = type;
        IsUsed = false;
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

    public void MarkAsUsed()
    {
        IsUsed = true;
    }
}