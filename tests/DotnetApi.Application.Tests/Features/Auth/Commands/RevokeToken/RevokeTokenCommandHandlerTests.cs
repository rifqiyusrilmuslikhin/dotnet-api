using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Application.Features.Auth.Commands.RevokeToken;
using DotnetApi.Domain.Interfaces;
using DotnetApi.Domain.Tests.Helpers;
using Shouldly;
using NSubstitute;
using RefreshTokenEntity = DotnetApi.Domain.Entities.RefreshToken;

namespace DotnetApi.Application.Tests.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly RevokeTokenCommandHandler _sut;

    public RevokeTokenCommandHandlerTests()
    {
        _sut = new RevokeTokenCommandHandler(_refreshTokenRepository);
    }

    [Fact]
    public async Task Handle_WithActiveToken_ShouldRevokeToken()
    {
        // Arrange
        var token = RefreshTokenFactory.Create(userId: 1, token: "active_token");
        _refreshTokenRepository.GetByTokenAsync("active_token", Arg.Any<CancellationToken>())
            .Returns(token);
        _refreshTokenRepository.UpdateAsync(token, Arg.Any<CancellationToken>())
            .Returns(token);

        // Act
        await _sut.Handle(new RevokeTokenCommand("active_token"), CancellationToken.None);

        // Assert
        token.IsRevoked.ShouldBeTrue();
        await _refreshTokenRepository.Received(1).UpdateAsync(token, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentToken_ShouldThrowValidationException()
    {
        // Arrange
        _refreshTokenRepository.GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RefreshTokenEntity?)null);

        var act = () => _sut.Handle(new RevokeTokenCommand("not_found"), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ValidationException>(act)
            .ContinueWith(t => t.Result.Message.ShouldContain("Invalid refresh token"));
    }

    [Fact]
    public async Task Handle_WithAlreadyRevokedToken_ShouldThrowValidationException()
    {
        // Arrange
        var revokedToken = RefreshTokenFactory.CreateRevoked(userId: 1, token: "revoked_token");
        _refreshTokenRepository.GetByTokenAsync("revoked_token", Arg.Any<CancellationToken>())
            .Returns(revokedToken);

        var act = () => _sut.Handle(new RevokeTokenCommand("revoked_token"), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ValidationException>(act)
            .ContinueWith(t => t.Result.Message.ShouldContain("already been revoked"));
    }
}
