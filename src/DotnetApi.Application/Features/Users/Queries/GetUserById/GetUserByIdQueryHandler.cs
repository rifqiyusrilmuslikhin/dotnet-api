using DotnetApi.Application.Common.Models;
using DotnetApi.Domain.Exceptions;
using DotnetApi.Domain.Interfaces;
using MediatR;

namespace DotnetApi.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Handles retrieval of a single user by ID
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc/>
    public async Task<UserResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.Id);

        return new UserResponse(
            Id: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt
        );
    }
}
