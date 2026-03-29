using DotnetApi.Application.Features.Auth.Commands.Login;
using FluentValidation.TestHelper;

namespace DotnetApi.Application.Tests.Features.Auth.Commands.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _sut = new();

    private static LoginCommand Valid() => new("john@example.com", "Password1");

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        var result = _sut.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_ShouldFail(string email)
    {
        var result = _sut.TestValidate(Valid() with { Email = email });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmptyPassword_ShouldFail()
    {
        var result = _sut.TestValidate(Valid() with { Password = "" });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
