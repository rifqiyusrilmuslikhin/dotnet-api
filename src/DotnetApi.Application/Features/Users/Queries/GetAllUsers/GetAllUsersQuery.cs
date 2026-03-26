using DotnetApi.Application.Common.Models;
using MediatR;

namespace DotnetApi.Application.Features.Users.Queries.GetAllUsers;

/// <summary>
/// Query to retrieve all registered users
/// </summary>
public record GetAllUsersQuery : IRequest<IReadOnlyList<UserResponse>>;
