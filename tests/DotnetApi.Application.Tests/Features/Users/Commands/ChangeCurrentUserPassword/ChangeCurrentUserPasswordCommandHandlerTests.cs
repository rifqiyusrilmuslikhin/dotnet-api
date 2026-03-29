using DotnetApi.Application.Common.Exceptions;
using DotnetApi.Application.Features.Users.Commands.ChangeCurrentUserPassword;
using DotnetApi.Domain.Exceptions;
using DotnetApi.Domain.Interfaces;
using DotnetApi.Domain.Tests.Helpers;
using Shouldly;
using NSubstitute;
using User = DotnetApi.Domain.Entities.User;

namespace DotnetApi.Application.Tests.Features.Users.Commands.ChangeCurrentUserPassword;

public class ChangeCurrentUserPasswordCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ChangeCurrentUserPasswordCommandHandler _sut;

    public ChangeCurrentUserPasswordCommandHandlerTests()
    {
        _sut = new ChangeCurrentUserPasswordCommandHandler(_userRepository, _passwordHasher);
    }

    private static ChangeCurrentUserPasswordCommand ValidCommand(int userId = 1) => new(
        UserId: userId,
        CurrentPassword: "OldPassword1",
        NewPassword: "NewPassword1",
        ConfirmNewPassword: "NewPassword1"
    );

    [Fact]
    public async Task Handle_WithCorrectCurrentPassword_ShouldUpdatePassword()
    {
        // Arrange
        var user = UserFactory.Create(id: 1, password: "hashed_old");
        var command = ValidCommand(userId: 1);

        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(command.CurrentPassword, user.Password).Returns(true);
        _passwordHasher.Hash(command.NewPassword).Returns("hashed_new");
        _userRepository.UpdateAsync(user, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        _passwordHasher.Received(1).Hash(command.NewPassword);
    }

    [Fact]
    public async Task Handle_WithWrongCurrentPassword_ShouldThrowValidationException()
    {
        // Arrange
        var user = UserFactory.Create(id: 1);
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var act = () => _sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<ValidationException>(act)
            .ContinueWith(t => t.Result.Message.ShouldContain("Current password is incorrect"));
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => _sut.Handle(ValidCommand(userId: 99), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<NotFoundException>(act);
    }
}
