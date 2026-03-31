using DotnetApi.Domain.Entities;
using DotnetApi.Domain.Enums;

namespace DotnetApi.Domain.Tests.Helpers;

/// <summary>
/// Factory untuk membuat test instance dari UserAccount entity
/// </summary>
public static class UserAccountFactory
{
    public static UserAccount CreateLocal(
        int id = 1,
        int userId = 1,
        string email = "test@example.com",
        string passwordHash = "hashed_password")
    {
        var account = UserAccount.CreateLocal(userId, email, passwordHash);

        typeof(UserAccount)
            .GetProperty(nameof(UserAccount.Id))!
            .SetValue(account, id);

        return account;
    }

    public static UserAccount CreateOAuth(
        int id = 1,
        int userId = 1,
        AuthProvider provider = AuthProvider.Google,
        string providerKey = "google-sub-123")
    {
        var account = UserAccount.CreateOAuth(userId, provider, providerKey);

        typeof(UserAccount)
            .GetProperty(nameof(UserAccount.Id))!
            .SetValue(account, id);

        return account;
    }
}
