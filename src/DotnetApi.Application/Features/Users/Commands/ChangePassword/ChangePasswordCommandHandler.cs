using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Domain.Exceptions;
using DotnetApi.Domain.Interfaces;
using MediatR;

namespace DotnetApi.Application.Features.Users.Commands.ChangePassword;

/// <summary>
/// Handles changing a user's password
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    /// <inheritdoc/>
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.Id);

        var isCurrentPasswordValid = _passwordHasher.Verify(request.CurrentPassword, user.Password);
        if (!isCurrentPasswordValid)
            throw new ValidationException("Current password is incorrect.");

        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatePassword(newPasswordHash);

        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}
