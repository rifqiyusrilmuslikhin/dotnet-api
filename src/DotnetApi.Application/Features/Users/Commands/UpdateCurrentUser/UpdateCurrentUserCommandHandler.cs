using DotnetApi.Application.Common.Models;
using DotnetApi.Domain.Exceptions;
using DotnetApi.Domain.Interfaces;
using MediatR;

namespace DotnetApi.Application.Features.Users.Commands.UpdateCurrentUser;

/// <summary>
/// Handles updating the currently authenticated user's profile
/// </summary>
public class UpdateCurrentUserCommandHandler : IRequestHandler<UpdateCurrentUserCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;

    public UpdateCurrentUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc/>
    public async Task<UserResponse> Handle(UpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        user.UpdateProfile(request.FullName);

        await _userRepository.UpdateAsync(user, cancellationToken);

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
