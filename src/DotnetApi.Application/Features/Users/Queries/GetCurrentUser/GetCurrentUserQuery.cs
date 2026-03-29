using DotnetApi.Application.Common.Models;
using MediatR;

namespace DotnetApi.Application.Features.Users.Queries.GetCurrentUser;

/// <summary>
/// Query to retrieve the currently authenticated user's profile
/// </summary>
public record GetCurrentUserQuery(int UserId) : IRequest<UserResponse>;
