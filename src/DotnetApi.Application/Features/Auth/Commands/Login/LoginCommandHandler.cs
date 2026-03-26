using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Application.Common.Models;
using DotnetApi.Domain.Interfaces;
using MediatR;

namespace DotnetApi.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handles user login and returns a JWT token upon success
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.Password))
            throw new ValidationException("Invalid email or password.");

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse(
            AccessToken: token,
            TokenType: "Bearer",
            ExpiresIn: 3600,
            UserId: user.Id,
            Email: user.Email,
            FullName: user.FullName
        );
    }
}
