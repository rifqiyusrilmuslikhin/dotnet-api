using DotnetApi.Domain.Entities;

namespace DotnetApi.Domain.Tests.Helpers;

/// <summary>
/// Factory for creating test instances of the User entity.
/// Uses reflection to set private properties since the constructor and all setters are private.
/// </summary>
public static class UserFactory
{
    public static User Create(
        int id = 1,
        string email = "test@example.com",
        string fullName = "Test User",
        string? avatar = null)
    {
        var user = User.Create(email, fullName);

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
