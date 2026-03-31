using DotnetApi.Domain.Exceptions;

namespace DotnetApi.Domain.Entities;

/// <summary>
/// Represents an application user (identity/profile only).
/// Authentication credentials are stored separately in <see cref="UserAccount"/>.
/// </summary>
public class User
{
    public int Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? Avatar { get; private set; }

    // Navigation
    public IReadOnlyCollection<UserAccount> Accounts { get; private set; } = new List<UserAccount>();
    public IReadOnlyCollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    // EF Core constructor
    private User() { }

    public static User Create(string email, string fullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        if (!email.Contains('@'))
            throw new DomainException("Email format is invalid.");

        return new User
        {
            Email = email.Trim().ToLowerInvariant(),
            FullName = fullName.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateProfile(string fullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        FullName = fullName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAvatar(string? avatarPath)
    {
        Avatar = avatarPath;
        UpdatedAt = DateTime.UtcNow;
    }
}
