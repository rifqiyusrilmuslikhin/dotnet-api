using DotnetApi.Application.Features.Auth.Commands.RefreshToken;
using FluentValidation.TestHelper;

namespace DotnetApi.Application.Tests.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidToken_ShouldNotHaveErrors()
    {
        var command = new RefreshTokenCommand("valid-refresh-token");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyToken_ShouldHaveError(string? token)
    {
        var command = new RefreshTokenCommand(token!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}
