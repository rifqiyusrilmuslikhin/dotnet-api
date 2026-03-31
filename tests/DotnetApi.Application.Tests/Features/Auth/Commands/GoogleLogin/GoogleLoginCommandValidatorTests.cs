using DotnetApi.Application.Features.Auth.Commands.GoogleLogin;
using FluentValidation.TestHelper;

namespace DotnetApi.Application.Tests.Features.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandValidatorTests
{
    private readonly GoogleLoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidToken_ShouldNotHaveErrors()
    {
        var command = new GoogleLoginCommand("valid-google-id-token");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Validate_WithEmptyOrNullToken_ShouldHaveError(string? token)
    {
        var command = new GoogleLoginCommand(token!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.IdToken);
    }
}
