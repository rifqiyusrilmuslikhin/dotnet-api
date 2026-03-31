using DotnetApi.Domain.Entities;
using DotnetApi.Domain.Enums;
using Shouldly;

namespace DotnetApi.Domain.Tests.Entities;

public class UserAccountEntityTests
{
    // ─── CreateLocal ─────────────────────────────────────────────────────────

    [Fact]
    public void CreateLocal_WithValidData_ShouldReturnAccount()
    {
        var account = UserAccount.CreateLocal(1, "john@example.com", "hashed_password");

        account.UserId.ShouldBe(1);
        account.Provider.ShouldBe(AuthProvider.Local);
        account.ProviderKey.ShouldBe("john@example.com");
        account.PasswordHash.ShouldBe("hashed_password");
        account.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
        account.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void CreateLocal_ShouldNormalizeProviderKeyToLowercase()
    {
        var account = UserAccount.CreateLocal(1, "JOHN@EXAMPLE.COM", "hash");

        account.ProviderKey.ShouldBe("john@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void CreateLocal_WithEmptyEmail_ShouldThrowArgumentException(string email)
    {
        var act = () => UserAccount.CreateLocal(1, email, "hash");

        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void CreateLocal_WithEmptyPasswordHash_ShouldThrowArgumentException(string hash)
    {
        var act = () => UserAccount.CreateLocal(1, "john@example.com", hash);

        Should.Throw<ArgumentException>(act);
    }

    // ─── CreateOAuth ─────────────────────────────────────────────────────────

    [Fact]
    public void CreateOAuth_WithValidData_ShouldReturnAccount()
    {
        var account = UserAccount.CreateOAuth(1, AuthProvider.Google, "google-sub-123");

        account.UserId.ShouldBe(1);
        account.Provider.ShouldBe(AuthProvider.Google);
        account.ProviderKey.ShouldBe("google-sub-123");
        account.PasswordHash.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void CreateOAuth_WithEmptyProviderKey_ShouldThrowArgumentException(string key)
    {
        var act = () => UserAccount.CreateOAuth(1, AuthProvider.Google, key);

        Should.Throw<ArgumentException>(act);
    }

    // ─── UpdatePassword ───────────────────────────────────────────────────────

    [Fact]
    public void UpdatePassword_WithValidHash_ShouldUpdatePasswordHash()
    {
        var account = UserAccount.CreateLocal(1, "john@example.com", "old_hash");

        account.UpdatePassword("new_hash");

        account.PasswordHash.ShouldBe("new_hash");
        account.UpdatedAt.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void UpdatePassword_WithEmptyHash_ShouldThrowArgumentException(string hash)
    {
        var account = UserAccount.CreateLocal(1, "john@example.com", "old_hash");

        var act = () => account.UpdatePassword(hash);

        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void UpdatePassword_OnOAuthAccount_ShouldThrowInvalidOperationException()
    {
        var account = UserAccount.CreateOAuth(1, AuthProvider.Google, "google-sub-123");

        var act = () => account.UpdatePassword("new_hash");

        Should.Throw<InvalidOperationException>(act);
    }
}
