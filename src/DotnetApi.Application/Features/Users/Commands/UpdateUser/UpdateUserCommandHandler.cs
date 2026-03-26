using DotnetApi.Application.Common.Models;
using DotnetApi.Domain.Exceptions;
using DotnetApi.Domain.Interfaces;
using MediatR;

namespace DotnetApi.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// Handles updating a user's profile information
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc/>
    public async Task<UserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.Id);

        user.UpdateProfile(request.FullName);

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new UserResponse(
            Id: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt
        );
    }
}
