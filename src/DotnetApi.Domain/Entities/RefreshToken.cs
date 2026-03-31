using DotnetApi.Domain.Exceptions;

namespace DotnetApi.Domain.Entities;

/// <summary>
/// Represents a refresh token used to obtain new access tokens.
/// Stored in the database and can be revoked.
/// </summary>
public class RefreshToken
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// When this token is rotated, stores the replacement token value.
    /// </summary>
    public string? ReplacedByToken { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;

    // Computed
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsRevoked && !IsExpired;

    // EF Core constructor
    private RefreshToken() { }

    /// <summary>
    /// Creates a new refresh token for the given user.
    /// </summary>
    public static RefreshToken Create(int userId, string token, int expiresInDays)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        if (userId <= 0)
            throw new DomainException("UserId must be greater than zero.");

        if (expiresInDays <= 0)
            throw new DomainException("Expiration days must be greater than zero.");

        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(expiresInDays),
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Revokes this refresh token, optionally recording the replacement token.
    /// </summary>
    public void Revoke(string? replacedByToken = null)
    {
        if (IsRevoked)
            throw new DomainException("Token is already revoked.");

        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
    }
}
