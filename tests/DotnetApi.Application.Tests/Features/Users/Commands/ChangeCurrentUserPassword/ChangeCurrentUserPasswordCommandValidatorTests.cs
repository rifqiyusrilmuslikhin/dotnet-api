using DotnetApi.Application.Features.Users.Commands.ChangeCurrentUserPassword;
using FluentValidation.TestHelper;

namespace DotnetApi.Application.Tests.Features.Users.Commands.ChangeCurrentUserPassword;

public class ChangeCurrentUserPasswordCommandValidatorTests
{
    private readonly ChangeCurrentUserPasswordCommandValidator _sut = new();

    private static ChangeCurrentUserPasswordCommand Valid() => new(
        UserId: 1,
        CurrentPassword: "OldPassword1",
        NewPassword: "NewPassword1",
        ConfirmNewPassword: "NewPassword1"
    );

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        var result = _sut.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyCurrentPassword_ShouldFail()
    {
        var result = _sut.TestValidate(Valid() with { CurrentPassword = "" });
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Theory]
    [InlineData("short1")]
    [InlineData("alllowercase1")]
    [InlineData("ALLUPPERCASE1")]
    [InlineData("NoDigitsHere")]
    public void Validate_WithWeakNewPassword_ShouldFail(string password)
    {
        var result = _sut.TestValidate(Valid() with { NewPassword = password, ConfirmNewPassword = password });
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Validate_WhenNewPasswordSameAsCurrent_ShouldFail()
    {
        var result = _sut.TestValidate(Valid() with { NewPassword = "OldPassword1", ConfirmNewPassword = "OldPassword1" });
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Validate_WhenConfirmPasswordMismatch_ShouldFail()
    {
        var result = _sut.TestValidate(Valid() with { ConfirmNewPassword = "DifferentPass1" });
        result.ShouldHaveValidationErrorFor(x => x.ConfirmNewPassword);
    }
}
