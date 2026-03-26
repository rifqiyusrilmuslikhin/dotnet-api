using DotnetApi.Application.Common.Models;
using DotnetApi.Domain.Interfaces;
using MediatR;

namespace DotnetApi.Application.Features.Users.Queries.GetAllUsers;

/// <summary>
/// Handles retrieval of all users
/// </summary>
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IReadOnlyList<UserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UserResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return users.Select(user => new UserResponse(
            Id: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt
        )).ToList();
    }
}
