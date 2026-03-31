using DotnetApi.Application.Features.Auth.Commands.RevokeToken;
using FluentValidation.TestHelper;

namespace DotnetApi.Application.Tests.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandValidatorTests
{
    private readonly RevokeTokenCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidToken_ShouldNotHaveErrors()
    {
        var command = new RevokeTokenCommand("valid-refresh-token");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyToken_ShouldHaveError(string? token)
    {
        var command = new RevokeTokenCommand(token!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}
