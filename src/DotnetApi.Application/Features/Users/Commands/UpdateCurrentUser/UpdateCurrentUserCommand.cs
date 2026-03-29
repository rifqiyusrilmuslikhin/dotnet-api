using DotnetApi.Application.Common.Models;
using MediatR;

namespace DotnetApi.Application.Features.Users.Commands.UpdateCurrentUser;

/// <summary>
/// Command to update the currently authenticated user's profile
/// </summary>
public record UpdateCurrentUserCommand(int UserId, string FullName) : IRequest<UserResponse>;
