using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Application.Features.Auth.Commands.RefreshToken;
using DotnetApi.Domain.Interfaces;
using DotnetApi.Domain.Tests.Helpers;
using Shouldly;
using NSubstitute;
using User = DotnetApi.Domain.Entities.User;
using RefreshTokenEntity = DotnetApi.Domain.Entities.RefreshToken;

namespace DotnetApi.Application.Tests.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly RefreshTokenCommandHandler _sut;

    public RefreshTokenCommandHandlerTests()
    {
        _sut = new RefreshTokenCommandHandler(
            _refreshTokenRepository, _userRepository, _jwtTokenService);

        _jwtTokenService.GenerateRefreshToken().Returns("new_refresh_token");
        _jwtTokenService.RefreshTokenExpiresInDays.Returns(7);
        _refreshTokenRepository.AddAsync(Arg.Any<RefreshTokenEntity>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<RefreshTokenEntity>());
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldReturnNewTokenPair()
    {
        // Arrange
        var user = UserFactory.Create(id: 1, email: "john@example.com", fullName: "John Doe");
        var existingToken = RefreshTokenFactory.Create(id: 1, userId: 1, token: "old_token");

        _refreshTokenRepository.GetByTokenAsync("old_token", Arg.Any<CancellationToken>())
            .Returns(existingToken);
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(user);
        _jwtTokenService.GenerateToken(user).Returns("new_access_token");
        _refreshTokenRepository.UpdateAsync(existingToken, Arg.Any<CancellationToken>())
            .Returns(existingToken);

        // Act
        var result = await _sut.Handle(new RefreshTokenCommand("old_token"), CancellationToken.None);

        // Assert
        result.AccessToken.ShouldBe("new_access_token");
        result.RefreshToken.ShouldBe("new_refresh_token");
        result.TokenType.ShouldBe("Bearer");
        result.UserId.ShouldBe(1);
        result.Email.ShouldBe("john@example.com");
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldRevokeOldToken()
    {
        // Arrange
        var user = UserFactory.Create(id: 1);
        var existingToken = RefreshTokenFactory.Create(id: 1, userId: 1, token: "old_token");

        _refreshTokenRepository.GetByTokenAsync("old_token", Arg.Any<CancellationToken>())
            .Returns(existingToken);
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(user);
        _jwtTokenService.GenerateToken(user).Returns("new_access_token");
        _refreshTokenRepository.UpdateAsync(existingToken, Arg.Any<CancellationToken>())
            .Returns(existingToken);

        // Act
        await _sut.Handle(new RefreshTokenCommand("old_token"), CancellationToken.None);

        // Assert — old token should be revoked with replacement
        existingToken.IsRevoked.ShouldBeTrue();
        existingToken.ReplacedByToken.ShouldBe("new_refresh_token");
        await _refreshTokenRepository.Received(1).UpdateAsync(existingToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldPersistNewRefreshToken()
    {
        // Arrange
        var user = UserFactory.Create(id: 1);
        var existingToken = RefreshTokenFactory.Create(id: 1, userId: 1, token: "old_token");

        _refreshTokenRepository.GetByTokenAsync("old_token", Arg.Any<CancellationToken>())
            .Returns(existingToken);
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(user);
        _jwtTokenService.GenerateToken(user).Returns("new_access_token");
        _refreshTokenRepository.UpdateAsync(existingToken, Arg.Any<CancellationToken>())
            .Returns(existingToken);

        // Act
        await _sut.Handle(new RefreshTokenCommand("old_token"), CancellationToken.None);

        // Assert
        await _refreshTokenRepository.Received(1).AddAsync(
            Arg.Is<RefreshTokenEntity>(r => r.Token == "new_refresh_token"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentToken_ShouldThrowValidationException()
    {
        // Arrange
        _refreshTokenRepository.GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RefreshTokenEntity?)null);

        var act = () => _sut.Handle(new RefreshTokenCommand("not_found"), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ValidationException>(act)
            .ContinueWith(t => t.Result.Message.ShouldContain("Invalid refresh token"));
    }

    [Fact]
    public async Task Handle_WithRevokedToken_ShouldThrowValidationException()
    {
        // Arrange
        var revokedToken = RefreshTokenFactory.CreateRevoked(userId: 1, token: "revoked_token");
        _refreshTokenRepository.GetByTokenAsync("revoked_token", Arg.Any<CancellationToken>())
            .Returns(revokedToken);

        var act = () => _sut.Handle(new RefreshTokenCommand("revoked_token"), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ValidationException>(act)
            .ContinueWith(t => t.Result.Message.ShouldContain("revoked"));
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ShouldThrowValidationException()
    {
        // Arrange
        var expiredToken = RefreshTokenFactory.CreateExpired(userId: 1, token: "expired_token");
        _refreshTokenRepository.GetByTokenAsync("expired_token", Arg.Any<CancellationToken>())
            .Returns(expiredToken);

        var act = () => _sut.Handle(new RefreshTokenCommand("expired_token"), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ValidationException>(act)
            .ContinueWith(t => t.Result.Message.ShouldContain("expired"));
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowValidationException()
    {
        // Arrange
        var existingToken = RefreshTokenFactory.Create(userId: 999, token: "valid_token");

        _refreshTokenRepository.GetByTokenAsync("valid_token", Arg.Any<CancellationToken>())
            .Returns(existingToken);
        _userRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => _sut.Handle(new RefreshTokenCommand("valid_token"), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ValidationException>(act)
            .ContinueWith(t => t.Result.Message.ShouldContain("User not found"));
    }
}
