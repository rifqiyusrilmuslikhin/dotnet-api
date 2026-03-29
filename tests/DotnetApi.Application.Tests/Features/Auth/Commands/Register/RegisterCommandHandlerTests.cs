using DotnetApi.Application.Features.Auth.Commands.Register;
using DotnetApi.Domain.Entities;
using DotnetApi.Domain.Interfaces;
using DotnetApi.Domain.Tests.Helpers;
using DotnetApi.Application.Common.Exceptions;
using Shouldly;
using NSubstitute;

namespace DotnetApi.Application.Tests.Features.Auth.Commands.Register;

public class RegisterCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly RegisterCommandHandler _sut;

    public RegisterCommandHandlerTests()
    {
        _sut = new RegisterCommandHandler(_userRepository, _passwordHasher, _jwtTokenService);
    }

    private static RegisterCommand ValidCommand() => new(
        FullName: "John Doe",
        Email: "john@example.com",
        Password: "Password1",
        ConfirmPassword: "Password1"
    );

    [Fact]
    public async Task Handle_WithNewEmail_ShouldReturnAuthResponse()
    {
        // Arrange
        var command = ValidCommand();
        var user = UserFactory.Create(id: 1, email: command.Email, fullName: command.FullName);

        _userRepository.ExistsByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash(command.Password)
            .Returns("hashed_password");
        _userRepository.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _jwtTokenService.GenerateToken(Arg.Any<User>())
            .Returns("jwt_token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.ShouldBe("jwt_token");
        result.TokenType.ShouldBe("Bearer");
        result.Email.ShouldBe(command.Email);
        result.FullName.ShouldBe(command.FullName);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldThrowValidationException()
    {
        // Arrange
        _userRepository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = () => _sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ValidationException>(act)
            .ContinueWith(t => t.Result.Message.ShouldContain("already registered"));
    }

    [Fact]
    public async Task Handle_ShouldHashPasswordBeforeSaving()
    {
        // Arrange
        var command = ValidCommand();
        var user = UserFactory.Create(email: command.Email, fullName: command.FullName);

        _userRepository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash(command.Password).Returns("hashed_password");
        _userRepository.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(user);
        _jwtTokenService.GenerateToken(Arg.Any<User>()).Returns("token");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert — password hasher harus dipanggil
        _passwordHasher.Received(1).Hash(command.Password);
    }
}
