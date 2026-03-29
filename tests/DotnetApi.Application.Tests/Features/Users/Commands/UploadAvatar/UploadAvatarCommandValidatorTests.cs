using DotnetApi.Application.Features.Users.Commands.UploadAvatar;
using DotnetApi.Domain.Tests.Helpers;
using FluentValidation.TestHelper;

namespace DotnetApi.Application.Tests.Features.Users.Commands.UploadAvatar;

public class UploadAvatarCommandValidatorTests
{
    private readonly UploadAvatarCommandValidator _sut = new();

    private static UploadAvatarCommand Valid() => new(
        UserId: 1,
        File: new FakeUploadedFile("avatar.jpg", "image/jpeg", new byte[1024])
    );

    [Fact]
    public void Validate_WithValidJpeg_ShouldPass()
    {
        var result = _sut.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public void Validate_WithAllowedContentTypes_ShouldPass(string contentType)
    {
        var cmd = Valid() with { File = new FakeUploadedFile("avatar.png", contentType, new byte[512]) };
        var result = _sut.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithDisallowedContentType_ShouldFail()
    {
        var cmd = Valid() with { File = new FakeUploadedFile("file.pdf", "application/pdf", new byte[512]) };
        var result = _sut.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public void Validate_WithEmptyFile_ShouldFail()
    {
        var cmd = Valid() with { File = new FakeUploadedFile("avatar.jpg", "image/jpeg", []) };
        var result = _sut.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public void Validate_WithFileTooLarge_ShouldFail()
    {
        // 3 MB — exceeds 2 MB limit
        var cmd = Valid() with { File = new FakeUploadedFile("big.jpg", "image/jpeg", new byte[3 * 1024 * 1024]) };
        var result = _sut.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.File);
    }
}
