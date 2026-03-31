using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Domain.Enums;
using DotnetApi.Domain.Exceptions;
using DotnetApi.Domain.Interfaces;
using MediatR;

namespace DotnetApi.Application.Features.Users.Commands.ChangeCurrentUserPassword;

/// <summary>
/// Handles changing the currently authenticated user's password
/// </summary>
public class ChangeCurrentUserPasswordCommandHandler : IRequestHandler<ChangeCurrentUserPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangeCurrentUserPasswordCommandHandler(
        IUserRepository userRepository,
        IUserAccountRepository userAccountRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _userAccountRepository = userAccountRepository;
        _passwordHasher = passwordHasher;
    }

    /// <inheritdoc/>
    public async Task Handle(ChangeCurrentUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        var account = await _userAccountRepository.GetLocalByUserIdAsync(user.Id, cancellationToken)
            ?? throw new ValidationException("This account does not use a local password.");

        if (!_passwordHasher.Verify(request.CurrentPassword, account.PasswordHash!))
            throw new ValidationException("Current password is incorrect.");

        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);
        account.UpdatePassword(newPasswordHash);

        await _userAccountRepository.UpdateAsync(account, cancellationToken);
    }
}
