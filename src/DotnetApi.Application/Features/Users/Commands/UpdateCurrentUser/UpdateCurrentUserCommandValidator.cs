using FluentValidation;

namespace DotnetApi.Application.Features.Users.Commands.UpdateCurrentUser;

public class UpdateCurrentUserCommandValidator : AbstractValidator<UpdateCurrentUserCommand>
{
    public UpdateCurrentUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(256).WithMessage("Full name must not exceed 256 characters.");
    }
}
