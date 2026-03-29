using FluentValidation;

namespace DotnetApi.Application.Features.Users.Commands.UploadAvatar;

public class UploadAvatarCommandValidator : AbstractValidator<UploadAvatarCommand>
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB

    public UploadAvatarCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("Avatar file is required.")
            .Must(f => f.Length > 0).WithMessage("Avatar file must not be empty.")
            .Must(f => f.Length <= MaxFileSizeBytes)
                .WithMessage("Avatar file must not exceed 2 MB.")
            .Must(f => AllowedContentTypes.Contains(f.ContentType.ToLowerInvariant()))
                .WithMessage("Only JPEG, PNG, and WebP images are allowed.");
    }
}
