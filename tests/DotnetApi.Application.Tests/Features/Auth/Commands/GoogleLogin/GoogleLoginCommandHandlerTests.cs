using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Application.Common.Models;
using DotnetApi.Application.Features.Auth.Commands.GoogleLogin;
using DotnetApi.Domain.Entities;
using DotnetApi.Domain.Enums;
using DotnetApi.Domain.Interfaces;
using DotnetApi.Domain.Tests.Helpers;
using Shouldly;
using NSubstitute;
using User = DotnetApi.Domain.Entities.User;
using UserAccount = DotnetApi.Domain.Entities.UserAccount;
using RefreshTokenEntity = DotnetApi.Domain.Entities.RefreshToken;

namespace DotnetApi.Application.Tests.Features.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandHandlerTests
{
    private readonly IGoogleTokenValidator _googleTokenValidator = Substitute.For<IGoogleTokenValidator>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUserAccountRepository _userAccountRepository = Substitute.For<IUserAccountRepository>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly GoogleLoginCommandHandler _sut;

    public GoogleLoginCommandHandlerTests()
    {
        _sut = new GoogleLoginCommandHandler(
            _googleTokenValidator,
            _userRepository,
            _userAccountRepository,
            _jwtTokenService,
            _refreshTokenRepository);

        // Default refresh token setup for all tests
        _jwtTokenService.GenerateRefreshToken().Returns("refresh_token_value");
        _jwtTokenService.RefreshTokenExpiresInDays.Returns(7);
        _refreshTokenRepository.AddAsync(Arg.Any<RefreshTokenEntity>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<RefreshTokenEntity>());
    }

    private static GoogleLoginCommand ValidCommand() => new("valid-google-id-token");

    private static GoogleUserInfo ValidGoogleUser() => new(
        Subject: "google-sub-12345",
        Email: "john@gmail.com",
        FullName: "John Doe"
    );

    // ─── Returning Google user (account already linked) ───────────────────────

    [Fact]
    public async Task Handle_WithExistingGoogleAccount_ShouldReturnAuthResponse()
    {
        // Arrange
        var googleUser = ValidGoogleUser();
        var user = UserFactory.Create(id: 1, email: googleUser.Email, fullName: googleUser.FullName);
        var account = UserAccountFactory.CreateOAuth(userId: 1, provider: AuthProvider.Google, providerKey: googleUser.Subject);

        _googleTokenValidator.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(googleUser);
        _userAccountRepository.GetByProviderAsync(AuthProvider.Google, googleUser.Subject, Arg.Any<CancellationToken>())
            .Returns(account);
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(user);
        _jwtTokenService.GenerateToken(user)
            .Returns("jwt_token");

        // Act
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.AccessToken.ShouldBe("jwt_token");
        result.RefreshToken.ShouldBe("refresh_token_value");
        result.Email.ShouldBe(googleUser.Email);
        result.FullName.ShouldBe(googleUser.FullName);

        // Should NOT create new user or user account
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _userAccountRepository.DidNotReceive().AddAsync(Arg.Any<UserAccount>(), Arg.Any<CancellationToken>());
    }

    // ─── First Google login, user already exists by email ─────────────────────

    [Fact]
    public async Task Handle_WithExistingUserByEmail_ShouldLinkGoogleAccount()
    {
        // Arrange
        var googleUser = ValidGoogleUser();
        var existingUser = UserFactory.Create(id: 5, email: googleUser.Email, fullName: "Existing Name");

        _googleTokenValidator.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(googleUser);
        _userAccountRepository.GetByProviderAsync(AuthProvider.Google, googleUser.Subject, Arg.Any<CancellationToken>())
            .Returns((UserAccount?)null);
        _userRepository.GetByEmailAsync(googleUser.Email, Arg.Any<CancellationToken>())
            .Returns(existingUser);
        _userAccountRepository.AddAsync(Arg.Any<UserAccount>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<UserAccount>());
        _jwtTokenService.GenerateToken(existingUser)
            .Returns("jwt_token");

        // Act
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.AccessToken.ShouldBe("jwt_token");
        result.RefreshToken.ShouldBe("refresh_token_value");
        result.UserId.ShouldBe(5);

        // Should NOT create a new user, but SHOULD link account
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _userAccountRepository.Received(1).AddAsync(
            Arg.Is<UserAccount>(a => a.Provider == AuthProvider.Google && a.ProviderKey == googleUser.Subject),
            Arg.Any<CancellationToken>());
    }

    // ─── First Google login, brand-new user ───────────────────────────────────

    [Fact]
    public async Task Handle_WithNewUser_ShouldCreateUserAndLinkGoogleAccount()
    {
        // Arrange
        var googleUser = ValidGoogleUser();
        var newUser = UserFactory.Create(id: 10, email: googleUser.Email, fullName: googleUser.FullName);

        _googleTokenValidator.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(googleUser);
        _userAccountRepository.GetByProviderAsync(AuthProvider.Google, googleUser.Subject, Arg.Any<CancellationToken>())
            .Returns((UserAccount?)null);
        _userRepository.GetByEmailAsync(googleUser.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(newUser);
        _userAccountRepository.AddAsync(Arg.Any<UserAccount>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<UserAccount>());
        _jwtTokenService.GenerateToken(Arg.Any<User>())
            .Returns("jwt_token");

        // Act
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.AccessToken.ShouldBe("jwt_token");
        result.RefreshToken.ShouldBe("refresh_token_value");
        result.UserId.ShouldBe(10);
        result.Email.ShouldBe(googleUser.Email);

        // Should create BOTH user and account
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _userAccountRepository.Received(1).AddAsync(
            Arg.Is<UserAccount>(a => a.Provider == AuthProvider.Google),
            Arg.Any<CancellationToken>());
    }

    // ─── Refresh token persisted ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_ShouldPersistRefreshToken()
    {
        // Arrange
        var googleUser = ValidGoogleUser();
        var user = UserFactory.Create(id: 1, email: googleUser.Email, fullName: googleUser.FullName);
        var account = UserAccountFactory.CreateOAuth(userId: 1, provider: AuthProvider.Google, providerKey: googleUser.Subject);

        _googleTokenValidator.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(googleUser);
        _userAccountRepository.GetByProviderAsync(AuthProvider.Google, googleUser.Subject, Arg.Any<CancellationToken>())
            .Returns(account);
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(user);
        _jwtTokenService.GenerateToken(user)
            .Returns("jwt_token");

        // Act
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        await _refreshTokenRepository.Received(1).AddAsync(
            Arg.Any<RefreshTokenEntity>(), Arg.Any<CancellationToken>());
    }

    // ─── Invalid token ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithInvalidGoogleToken_ShouldThrowValidationException()
    {
        // Arrange
        _googleTokenValidator.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((GoogleUserInfo?)null);

        var act = () => _sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ValidationException>(act)
            .ContinueWith(t => t.Result.Message.ShouldContain("Invalid Google ID token"));
    }
}
