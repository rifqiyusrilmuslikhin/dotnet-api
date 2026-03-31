using DotnetApi.Domain.Entities;
using DotnetApi.Domain.Exceptions;
using Shouldly;

namespace DotnetApi.Domain.Tests.Entities;

public class RefreshTokenEntityTests
{
    [Fact]
    public void Create_WithValidInputs_ShouldSetProperties()
    {
        // Act
        var token = RefreshToken.Create(1, "abc123", 7);

        // Assert
        token.UserId.ShouldBe(1);
        token.Token.ShouldBe("abc123");
        token.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow.AddDays(6));
        token.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-2), DateTime.UtcNow.AddSeconds(2));
        token.RevokedAt.ShouldBeNull();
        token.ReplacedByToken.ShouldBeNull();
        token.IsActive.ShouldBeTrue();
        token.IsRevoked.ShouldBeFalse();
        token.IsExpired.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidToken_ShouldThrowArgumentException(string? tokenValue)
    {
        var act = () => RefreshToken.Create(1, tokenValue!, 7);
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidUserId_ShouldThrowDomainException(int userId)
    {
        var act = () => RefreshToken.Create(userId, "abc123", 7);
        Should.Throw<DomainException>(act)
            .Message.ShouldContain("UserId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithInvalidExpiresInDays_ShouldThrowDomainException(int days)
    {
        var act = () => RefreshToken.Create(1, "abc123", days);
        Should.Throw<DomainException>(act)
            .Message.ShouldContain("Expiration");
    }

    [Fact]
    public void Revoke_WithActiveToken_ShouldSetRevokedAt()
    {
        // Arrange
        var token = RefreshToken.Create(1, "abc123", 7);

        // Act
        token.Revoke();

        // Assert
        token.IsRevoked.ShouldBeTrue();
        token.IsActive.ShouldBeFalse();
        token.RevokedAt.ShouldNotBeNull();
        token.ReplacedByToken.ShouldBeNull();
    }

    [Fact]
    public void Revoke_WithReplacementToken_ShouldSetReplacedByToken()
    {
        // Arrange
        var token = RefreshToken.Create(1, "old-token", 7);

        // Act
        token.Revoke("new-token");

        // Assert
        token.IsRevoked.ShouldBeTrue();
        token.ReplacedByToken.ShouldBe("new-token");
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldThrowDomainException()
    {
        // Arrange
        var token = RefreshToken.Create(1, "abc123", 7);
        token.Revoke();

        // Act
        var act = () => token.Revoke();

        // Assert
        Should.Throw<DomainException>(act)
            .Message.ShouldContain("already revoked");
    }
}
