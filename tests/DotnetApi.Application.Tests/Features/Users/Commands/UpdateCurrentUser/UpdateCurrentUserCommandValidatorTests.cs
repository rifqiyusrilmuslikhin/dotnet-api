using DotnetApi.Application.Features.Users.Commands.UpdateCurrentUser;
using FluentValidation.TestHelper;

namespace DotnetApi.Application.Tests.Features.Users.Commands.UpdateCurrentUser;

public class UpdateCurrentUserCommandValidatorTests
{
    private readonly UpdateCurrentUserCommandValidator _sut = new();

    private static UpdateCurrentUserCommand Valid() => new(UserId: 1, FullName: "John Doe");

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
        var result = _sut.TestValidate(Valid() with { FullName = new string('a', 257) });
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }
}
