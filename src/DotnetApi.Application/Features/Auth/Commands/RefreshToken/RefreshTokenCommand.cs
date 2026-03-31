using DotnetApi.Application.Common.Models;
using MediatR;

namespace DotnetApi.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;
