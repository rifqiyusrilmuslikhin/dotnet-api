using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Application.Common.Models;
using DotnetApi.Domain.Entities;
using DotnetApi.Domain.Interfaces;
using MediatR;

namespace DotnetApi.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handles user registration and returns a JWT token upon success
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (emailExists)
            throw new ValidationException("Email is already registered.");

        var hashedPassword = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, request.FullName, hashedPassword);

        await _userRepository.AddAsync(user, cancellationToken);

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
