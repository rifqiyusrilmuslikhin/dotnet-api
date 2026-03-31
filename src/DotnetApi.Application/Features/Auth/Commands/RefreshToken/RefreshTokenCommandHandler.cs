using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Application.Common.Models;
using DotnetApi.Domain.Interfaces;
using MediatR;

using RefreshTokenEntity = DotnetApi.Domain.Entities.RefreshToken;

namespace DotnetApi.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Handles refresh token rotation: validates the old refresh token, revokes it,
/// and issues a new access token + refresh token pair.
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the refresh token
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new ValidationException("Invalid refresh token.");

        // 2. Validate the token is still usable
        if (existingToken.IsRevoked)
            throw new ValidationException("Refresh token has been revoked.");

        if (existingToken.IsExpired)
            throw new ValidationException("Refresh token has expired.");

        // 3. Load the user
        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken)
            ?? throw new ValidationException("User not found.");

        // 4. Rotate: generate new tokens
        var newAccessToken = _jwtTokenService.GenerateToken(user);
        var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        // 5. Revoke the old token (record replacement)
        existingToken.Revoke(replacedByToken: newRefreshTokenValue);
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        // 6. Persist the new refresh token
        var newRefreshToken = RefreshTokenEntity.Create(user.Id, newRefreshTokenValue, _jwtTokenService.RefreshTokenExpiresInDays);
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        return new AuthResponse(
            AccessToken: newAccessToken,
            TokenType: "Bearer",
            ExpiresIn: 3600,
            RefreshToken: newRefreshTokenValue,
            UserId: user.Id,
            Email: user.Email,
            FullName: user.FullName
        );
    }
}
