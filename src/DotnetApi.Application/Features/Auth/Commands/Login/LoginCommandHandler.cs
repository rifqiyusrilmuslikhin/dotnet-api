using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Application.Common.Models;
using DotnetApi.Domain.Entities;
using DotnetApi.Domain.Enums;
using DotnetApi.Domain.Interfaces;
using MediatR;

using RefreshTokenEntity = DotnetApi.Domain.Entities.RefreshToken;

namespace DotnetApi.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handles user login and returns a JWT token upon success
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUserAccountRepository userAccountRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _userAccountRepository = userAccountRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // 2. Find local account by provider key (email)
        var account = user is not null
            ? await _userAccountRepository.GetByProviderAsync(AuthProvider.Local, request.Email, cancellationToken)
            : null;

        if (user is null || account is null || account.PasswordHash is null
            || !_passwordHasher.Verify(request.Password, account.PasswordHash))
            throw new ValidationException("Invalid email or password.");

        // 3. Generate tokens
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
