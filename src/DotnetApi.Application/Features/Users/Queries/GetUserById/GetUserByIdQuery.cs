using DotnetApi.Application.Common.Models;
using MediatR;

namespace DotnetApi.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Query to retrieve a single user by their ID
/// </summary>
public record GetUserByIdQuery(int Id) : IRequest<UserResponse>;
