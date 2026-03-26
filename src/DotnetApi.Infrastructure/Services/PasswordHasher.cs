using DotnetApi.Domain.Interfaces;

namespace DotnetApi.Infrastructure.Services;

/// <summary>
/// BCrypt-based password hasher using .NET's built-in BCrypt
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, 12);
    }

    public bool Verify(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
    }
}
