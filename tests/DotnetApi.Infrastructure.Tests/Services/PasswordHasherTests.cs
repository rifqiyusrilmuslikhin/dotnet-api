using DotnetApi.Infrastructure.Services;
using Shouldly;

namespace DotnetApi.Infrastructure.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_ShouldReturnNonEmptyString()
    {
        var hash = _sut.Hash("MyPassword1");

        hash.ShouldNotBeNullOrWhiteSpace();
        hash.ShouldNotBe("MyPassword1");
    }

    [Fact]
    public void Hash_SamePlaintext_ShouldProduceDifferentHashes()
    {
        // BCrypt uses a random salt per call
        var hash1 = _sut.Hash("MyPassword1");
        var hash2 = _sut.Hash("MyPassword1");

        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        var hash = _sut.Hash("MyPassword1");

        var result = _sut.Verify("MyPassword1", hash);

        result.ShouldBeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ShouldReturnFalse()
    {
        var hash = _sut.Hash("MyPassword1");

        var result = _sut.Verify("WrongPassword1", hash);

        result.ShouldBeFalse();
    }
}
