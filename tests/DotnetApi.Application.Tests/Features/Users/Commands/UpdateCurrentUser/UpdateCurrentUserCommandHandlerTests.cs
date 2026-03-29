using DotnetApi.Application.Features.Users.Commands.UpdateCurrentUser;
using DotnetApi.Domain.Exceptions;
using DotnetApi.Domain.Interfaces;
using DotnetApi.Domain.Tests.Helpers;
using Shouldly;
using NSubstitute;
using User = DotnetApi.Domain.Entities.User;

namespace DotnetApi.Application.Tests.Features.Users.Commands.UpdateCurrentUser;

public class UpdateCurrentUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly UpdateCurrentUserCommandHandler _sut;

    public UpdateCurrentUserCommandHandlerTests()
    {
        _sut = new UpdateCurrentUserCommandHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldUpdateAndReturnResponse()
    {
        // Arrange
        var user = UserFactory.Create(id: 1, email: "john@example.com", fullName: "Old Name");
        var command = new UpdateCurrentUserCommand(UserId: 1, FullName: "New Name");

        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.UpdateAsync(user, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.FullName.ShouldBe("New Name");
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => _sut.Handle(new UpdateCurrentUserCommand(99, "New Name"), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<NotFoundException>(act);
    }
}
