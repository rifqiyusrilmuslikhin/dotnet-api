using DotnetApi.Application.Features.Auth.Commands.Register;
using FluentValidation.TestHelper;

namespace DotnetApi.Application.Tests.Features.Auth.Commands.Register;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _sut = new();

    private static RegisterCommand Valid() => new(
        FullName: "John Doe",
        Email: "john@example.com",
        Password: "Password1",
        ConfirmPassword: "Password1"
    );

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        var result = _sut.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_WithEmptyFullName_ShouldFail(string name)
    {
        var result = _sut.TestValidate(Valid() with { FullName = name });
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_WithTooLongFullName_ShouldFail()
    {
        var result = _sut.TestValidate(Valid() with { FullName = new string('a', 101) });
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_ShouldFail(string email)
    {
        var result = _sut.TestValidate(Valid() with { Email = email });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("short1")]        // too short
    [InlineData("alllowercase1")] // no uppercase
    [InlineData("ALLUPPERCASE1")] // no lowercase
    [InlineData("NoDigitsHere")]  // no digit
    public void Validate_WithWeakPassword_ShouldFail(string password)
    {
        var result = _sut.TestValidate(Valid() with { Password = password, ConfirmPassword = password });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WhenPasswordsMismatch_ShouldFail()
    {
        var result = _sut.TestValidate(Valid() with { ConfirmPassword = "DifferentPass1" });
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }
}
