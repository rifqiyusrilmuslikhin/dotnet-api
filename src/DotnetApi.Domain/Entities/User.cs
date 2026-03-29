using DotnetApi.Domain.Exceptions;

namespace DotnetApi.Domain.Entities;

/// <summary>
/// Represents an application user
/// </summary>
public class User
{
    public int Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string Password { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? Avatar { get; private set; }

    // EF Core constructor
    private User() { }

    public static User Create(string email, string fullName, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        if (!email.Contains('@'))
            throw new DomainException("Email format is invalid.");

        return new User
        {
            Email = email.Trim().ToLowerInvariant(),
            FullName = fullName.Trim(),
            Password = password,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateProfile(string fullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        FullName = fullName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);

        Password = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAvatar(string? avatarPath)
    {
        Avatar = avatarPath;
        UpdatedAt = DateTime.UtcNow;
    }
}
