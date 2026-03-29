using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Application.Features.Auth.Commands.Login;
using DotnetApi.Domain.Interfaces;
using DotnetApi.Domain.Tests.Helpers;
using Shouldly;
using NSubstitute;
using User = DotnetApi.Domain.Entities.User;

namespace DotnetApi.Application.Tests.Features.Auth.Commands.Login;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        _sut = new LoginCommandHandler(_userRepository, _passwordHasher, _jwtTokenService);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var user = UserFactory.Create(id: 1, email: "john@example.com");
        var command = new LoginCommand("john@example.com", "Password1");

        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify(command.Password, user.Password)
            .Returns(true);
        _jwtTokenService.GenerateToken(user)
            .Returns("jwt_token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.ShouldBe("jwt_token");
        result.Email.ShouldBe(user.Email);
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
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>())
            .Returns(false);

        var act = () => _sut.Handle(new LoginCommand("john@example.com", "WrongPass1"), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ValidationException>(act)
            .ContinueWith(t => t.Result.Message.ShouldContain("Invalid email or password"));
    }
}
