using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Application.Common.Models;
using DotnetApi.Domain.Entities;
using DotnetApi.Domain.Enums;
using DotnetApi.Domain.Interfaces;
using MediatR;

using RefreshTokenEntity = DotnetApi.Domain.Entities.RefreshToken;

namespace DotnetApi.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handles user registration and returns a JWT token upon success
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RegisterCommandHandler(
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
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (emailExists)
            throw new ValidationException("Email is already registered.");

        // 1. Create user profile
        var user = User.Create(request.Email, request.FullName);
        user = await _userRepository.AddAsync(user, cancellationToken);

        // 2. Create local account with hashed password
        var hashedPassword = _passwordHasher.Hash(request.Password);
        var account = UserAccount.CreateLocal(user.Id, request.Email, hashedPassword);
        await _userAccountRepository.AddAsync(account, cancellationToken);

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
