using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Application.Features.Auth.Commands.Login;
using DotnetApi.Domain.Enums;
using DotnetApi.Domain.Interfaces;
using DotnetApi.Domain.Tests.Helpers;
using Shouldly;
using NSubstitute;
using User = DotnetApi.Domain.Entities.User;
using UserAccount = DotnetApi.Domain.Entities.UserAccount;
using RefreshTokenEntity = DotnetApi.Domain.Entities.RefreshToken;

namespace DotnetApi.Application.Tests.Features.Auth.Commands.Login;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUserAccountRepository _userAccountRepository = Substitute.For<IUserAccountRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        _sut = new LoginCommandHandler(
            _userRepository, _userAccountRepository, _passwordHasher,
            _jwtTokenService, _refreshTokenRepository);
    }

    private void SetupValidLogin(User user, UserAccount account, string password)
    {
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _userAccountRepository.GetByProviderAsync(AuthProvider.Local, user.Email, Arg.Any<CancellationToken>())
            .Returns(account);
        _passwordHasher.Verify(password, account.PasswordHash!).Returns(true);
        _jwtTokenService.GenerateToken(user).Returns("jwt_token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh_token_value");
        _jwtTokenService.RefreshTokenExpiresInDays.Returns(7);
        _refreshTokenRepository.AddAsync(Arg.Any<RefreshTokenEntity>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<RefreshTokenEntity>());
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var user = UserFactory.Create(id: 1, email: "john@example.com");
        var account = UserAccountFactory.CreateLocal(userId: 1, email: "john@example.com", passwordHash: "hashed");
        var command = new LoginCommand("john@example.com", "Password1");
        SetupValidLogin(user, account, command.Password);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.ShouldBe("jwt_token");
        result.RefreshToken.ShouldBe("refresh_token_value");
        result.Email.ShouldBe(user.Email);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldPersistRefreshToken()
    {
        // Arrange
        var user = UserFactory.Create(id: 1, email: "john@example.com");
        var account = UserAccountFactory.CreateLocal(userId: 1, email: "john@example.com", passwordHash: "hashed");
        var command = new LoginCommand("john@example.com", "Password1");
        SetupValidLogin(user, account, command.Password);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _refreshTokenRepository.Received(1).AddAsync(
            Arg.Any<RefreshTokenEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ShouldThrowValidationException()
    {
        // Arrange
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => _sut.Handle(new LoginCommand("nobody@example.com", "Password1"), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ValidationException>(act)
            .ContinueWith(t => t.Result.Message.ShouldContain("Invalid email or password"));
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldThrowValidationException()
    {
        // Arrange
        var user = UserFactory.Create(email: "john@example.com");
        var account = UserAccountFactory.CreateLocal(userId: 1, email: "john@example.com", passwordHash: "hashed");

        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _userAccountRepository.GetByProviderAsync(AuthProvider.Local, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(account);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>())
            .Returns(false);

        var act = () => _sut.Handle(new LoginCommand("john@example.com", "WrongPass1"), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ValidationException>(act)
            .ContinueWith(t => t.Result.Message.ShouldContain("Invalid email or password"));
    }

    [Fact]
    public async Task Handle_WithNoLocalAccount_ShouldThrowValidationException()
    {
        // Arrange
        var user = UserFactory.Create(email: "john@example.com");

        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _userAccountRepository.GetByProviderAsync(AuthProvider.Local, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((UserAccount?)null);

        var act = () => _sut.Handle(new LoginCommand("john@example.com", "Password1"), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ValidationException>(act)
            .ContinueWith(t => t.Result.Message.ShouldContain("Invalid email or password"));
    }
}
