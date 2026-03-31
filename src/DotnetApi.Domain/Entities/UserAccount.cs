using DotnetApi.Domain.Enums;

namespace DotnetApi.Domain.Entities;

/// <summary>
/// Represents an external or local authentication account linked to a user.
/// A single User can have multiple accounts (e.g., local + Google).
/// </summary>
public class UserAccount
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public AuthProvider Provider { get; private set; }

    /// <summary>
    /// The unique identifier from the provider (e.g., Google sub, or user email for local).
    /// </summary>
    public string ProviderKey { get; private set; } = null!;

    /// <summary>
    /// Hashed password — only set for Local provider accounts.
    /// </summary>
    public string? PasswordHash { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;

    // EF Core constructor
    private UserAccount() { }

    /// <summary>
    /// Creates a local (email + password) account.
    /// </summary>
    public static UserAccount CreateLocal(int userId, string email, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new UserAccount
        {
            UserId = userId,
            Provider = AuthProvider.Local,
            ProviderKey = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an OAuth account (e.g., Google).
    /// </summary>
    public static UserAccount CreateOAuth(int userId, AuthProvider provider, string providerKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);

        if (provider == AuthProvider.Local)
            throw new ArgumentException("Use CreateLocal() for local accounts.", nameof(provider));

        return new UserAccount
        {
            UserId = userId,
            Provider = provider,
            ProviderKey = providerKey.Trim(),
            PasswordHash = null,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the password hash (local accounts only).
    /// </summary>
    public void UpdatePassword(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);

        if (Provider != AuthProvider.Local)
            throw new InvalidOperationException("Cannot set password on a non-local account.");

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }
}
