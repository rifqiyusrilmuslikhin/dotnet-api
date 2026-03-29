using MediatR;

namespace DotnetApi.Application.Features.Users.Commands.ChangeCurrentUserPassword;

/// <summary>
/// Command to change the currently authenticated user's password
/// </summary>
public record ChangeCurrentUserPasswordCommand(
    int UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest;
