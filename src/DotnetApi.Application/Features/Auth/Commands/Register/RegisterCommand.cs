using DotnetApi.Application.Common.Models;
using MediatR;

namespace DotnetApi.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new user account
/// </summary>
public record RegisterCommand(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword
) : IRequest<AuthResponse>;
