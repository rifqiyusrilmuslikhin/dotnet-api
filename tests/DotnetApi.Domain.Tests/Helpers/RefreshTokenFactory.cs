using DotnetApi.Domain.Entities;

namespace DotnetApi.Domain.Tests.Helpers;

/// <summary>
/// Factory untuk membuat test instance dari RefreshToken entity
/// </summary>
public static class RefreshTokenFactory
{
    public static RefreshToken Create(
        int id = 1,
        int userId = 1,
        string token = "test-refresh-token",
        int expiresInDays = 7)
    {
        var refreshToken = RefreshToken.Create(userId, token, expiresInDays);

        typeof(RefreshToken)
            .GetProperty(nameof(RefreshToken.Id))!
            .SetValue(refreshToken, id);

        return refreshToken;
    }

    public static RefreshToken CreateExpired(
        int id = 1,
        int userId = 1,
        string token = "expired-refresh-token")
    {
        var refreshToken = RefreshToken.Create(userId, token, 1);

        typeof(RefreshToken)
            .GetProperty(nameof(RefreshToken.Id))!
            .SetValue(refreshToken, id);

        // Force ExpiresAt to the past
        typeof(RefreshToken)
            .GetProperty(nameof(RefreshToken.ExpiresAt))!
            .SetValue(refreshToken, DateTime.UtcNow.AddDays(-1));

        return refreshToken;
    }

    public static RefreshToken CreateRevoked(
        int id = 1,
        int userId = 1,
        string token = "revoked-refresh-token")
    {
        var refreshToken = Create(id, userId, token);
        refreshToken.Revoke();
        return refreshToken;
    }
}
