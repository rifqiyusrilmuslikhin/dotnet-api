using DotnetApi.Application.Common.Models;
using DotnetApi.Domain.Exceptions;
using DotnetApi.Domain.Interfaces;
using MediatR;

namespace DotnetApi.Application.Features.Users.Queries.GetCurrentUser;

/// <summary>
/// Handles retrieval of the currently authenticated user's profile
/// </summary>
public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserResponse>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc/>
    public async Task<UserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        return new UserResponse(
            Id: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt,
            Avatar: user.Avatar
        );
    }
}
