using DotnetApi.Domain.Entities;

namespace DotnetApi.Domain.Tests.Helpers;

/// <summary>
/// Factory untuk membuat test instance dari User entity
/// karena constructor-nya private dan semua setter private
/// </summary>
public static class UserFactory
{
    public static User Create(
        int id = 1,
        string email = "test@example.com",
        string fullName = "Test User",
        string password = "hashed_password",
        string? avatar = null)
    {
        var user = User.Create(email, fullName, password);

        // Set Id via reflection (private set)
        typeof(User)
            .GetProperty(nameof(User.Id))!
            .SetValue(user, id);

        if (avatar is not null)
        {
            typeof(User)
                .GetProperty(nameof(User.Avatar))!
                .SetValue(user, avatar);
        }

        return user;
    }
}
