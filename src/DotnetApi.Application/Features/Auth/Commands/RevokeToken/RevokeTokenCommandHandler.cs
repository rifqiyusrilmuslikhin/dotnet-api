using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Domain.Interfaces;
using MediatR;

namespace DotnetApi.Application.Features.Auth.Commands.RevokeToken;

/// <summary>
/// Handles revoking a refresh token so it can no longer be used.
/// </summary>
public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RevokeTokenCommandHandler(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    /// <inheritdoc/>
    public async Task Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new ValidationException("Invalid refresh token.");

        if (token.IsRevoked)
            throw new ValidationException("Refresh token has already been revoked.");

        token.Revoke();
        await _refreshTokenRepository.UpdateAsync(token, cancellationToken);
    }
}
