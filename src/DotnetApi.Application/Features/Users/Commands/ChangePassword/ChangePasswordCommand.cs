using MediatR;

namespace DotnetApi.Application.Features.Users.Commands.ChangePassword;

/// <summary>
/// Command to change a user's password
/// </summary>
public record ChangePasswordCommand(
    int Id,
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest;
