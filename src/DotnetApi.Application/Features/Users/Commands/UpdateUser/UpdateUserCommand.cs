using DotnetApi.Application.Common.Models;
using MediatR;

namespace DotnetApi.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// Command to update a user's profile information
/// </summary>
public record UpdateUserCommand(int Id, string FullName) : IRequest<UserResponse>;
