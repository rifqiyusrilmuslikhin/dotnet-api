using DotnetApi.Domain.Entities;
using DotnetApi.Domain.Exceptions;
using Shouldly;

namespace DotnetApi.Domain.Tests.Entities;

public class UserEntityTests
{
    // ─── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ShouldReturnUser()
    {
        var user = User.Create("john@example.com", "John Doe");

        user.Email.ShouldBe("john@example.com");
        user.FullName.ShouldBe("John Doe");
        user.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
        user.UpdatedAt.ShouldBeNull();
        user.Avatar.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldNormalizeEmailToLowercase()
    {
        var user = User.Create("JOHN@EXAMPLE.COM", "John Doe");

        user.Email.ShouldBe("john@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithEmptyEmail_ShouldThrowArgumentException(string email)
    {
        var act = () => User.Create(email, "John Doe");

        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithEmptyFullName_ShouldThrowArgumentException(string fullName)
    {
        var act = () => User.Create("john@example.com", fullName);

        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrowDomainException()
    {
        var act = () => User.Create("not-an-email", "John Doe");

        var ex = Should.Throw<DomainException>(act);
        ex.Message.ToLower().ShouldContain("email");
    }

    // ─── UpdateProfile ────────────────────────────────────────────────────────

    [Fact]
    public void UpdateProfile_WithValidName_ShouldUpdateFullName()
    {
        var user = User.Create("john@example.com", "John Doe");

        user.UpdateProfile("Jane Smith");

        user.FullName.ShouldBe("Jane Smith");
        user.UpdatedAt.ShouldNotBeNull();
        user.UpdatedAt!.Value.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void UpdateProfile_WithEmptyName_ShouldThrowArgumentException(string fullName)
    {
        var user = User.Create("john@example.com", "John Doe");

        var act = () => user.UpdateProfile(fullName);

        Should.Throw<ArgumentException>(act);
    }

    // ─── UpdateAvatar ─────────────────────────────────────────────────────────

    [Fact]
    public void UpdateAvatar_WithValidPath_ShouldSetAvatar()
    {
        var user = User.Create("john@example.com", "John Doe");

        user.UpdateAvatar("/uploads/avatars/1_abc.jpg");

        user.Avatar.ShouldBe("/uploads/avatars/1_abc.jpg");
        user.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void UpdateAvatar_WithNull_ShouldClearAvatar()
    {
        var user = User.Create("john@example.com", "John Doe");
        user.UpdateAvatar("/uploads/avatars/1_abc.jpg");

        user.UpdateAvatar(null);

        user.Avatar.ShouldBeNull();
    }
}

