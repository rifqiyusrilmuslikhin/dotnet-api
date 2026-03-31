using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Application.Common.Models;
using DotnetApi.Domain.Entities;
using DotnetApi.Domain.Enums;
using DotnetApi.Domain.Interfaces;
using MediatR;

using RefreshTokenEntity = DotnetApi.Domain.Entities.RefreshToken;

namespace DotnetApi.Application.Features.Auth.Commands.GoogleLogin;

/// <summary>
/// Handles Google OAuth2 login: validates the ID token, creates or finds the user,
/// links the Google account, and returns a JWT token.
/// </summary>
public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, AuthResponse>
{
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IUserRepository _userRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public GoogleLoginCommandHandler(
        IGoogleTokenValidator googleTokenValidator,
        IUserRepository userRepository,
        IUserAccountRepository userAccountRepository,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _googleTokenValidator = googleTokenValidator;
        _userRepository = userRepository;
        _userAccountRepository = userAccountRepository;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate Google ID token
        var googleUser = await _googleTokenValidator.ValidateAsync(request.IdToken, cancellationToken)
            ?? throw new ValidationException("Invalid Google ID token.");

        // 2. Check if a Google account already exists for this subject
        var existingAccount = await _userAccountRepository.GetByProviderAsync(
            AuthProvider.Google, googleUser.Subject, cancellationToken);

        User user;

        if (existingAccount is not null)
        {
            // 3a. Returning Google user — load their profile
            user = (await _userRepository.GetByIdAsync(existingAccount.UserId, cancellationToken))!;
        }
        else
        {
            // 3b. First-time Google login — find or create user profile
            var existingUser = await _userRepository.GetByEmailAsync(googleUser.Email, cancellationToken);

            if (existingUser is not null)
            {
                // User exists (registered via local or another provider) — link the Google account
                user = existingUser;
            }
            else
            {
                // Brand-new user — create profile
                var newUser = User.Create(googleUser.Email, googleUser.FullName);
                user = await _userRepository.AddAsync(newUser, cancellationToken);
            }

            // Link Google account to the user
            var account = UserAccount.CreateOAuth(user.Id, AuthProvider.Google, googleUser.Subject);
            await _userAccountRepository.AddAsync(account, cancellationToken);
        }

        // 4. Generate tokens
        var accessToken = _jwtTokenService.GenerateToken(user);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var refreshToken = RefreshTokenEntity.Create(user.Id, refreshTokenValue, _jwtTokenService.RefreshTokenExpiresInDays);
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        return new AuthResponse(
            AccessToken: accessToken,
            TokenType: "Bearer",
            ExpiresIn: 3600,
            RefreshToken: refreshTokenValue,
            UserId: user.Id,
            Email: user.Email,
            FullName: user.FullName
        );
    }
}
