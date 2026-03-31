using DotnetApi.Application.Common.Models;
using MediatR;

namespace DotnetApi.Application.Features.Auth.Commands.GoogleLogin;

/// <summary>
/// Command to authenticate or register a user via Google OAuth2 ID token
/// </summary>
public record GoogleLoginCommand(
    string IdToken
) : IRequest<AuthResponse>;
