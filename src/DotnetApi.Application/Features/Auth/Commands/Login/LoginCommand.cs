using DotnetApi.Application.Common.Models;
using MediatR;

namespace DotnetApi.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user and receive a JWT token
/// </summary>
public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;
