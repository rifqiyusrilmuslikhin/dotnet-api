using DotnetApi.Application.Features.Users.Queries.GetCurrentUser;
using DotnetApi.Domain.Exceptions;
using DotnetApi.Domain.Interfaces;
using DotnetApi.Domain.Tests.Helpers;
using Shouldly;
using NSubstitute;
using User = DotnetApi.Domain.Entities.User;

namespace DotnetApi.Application.Tests.Features.Users.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly GetCurrentUserQueryHandler _sut;

    public GetCurrentUserQueryHandlerTests()
    {
        _sut = new GetCurrentUserQueryHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldReturnUserResponse()
    {
        // Arrange
        var user = UserFactory.Create(id: 1, email: "john@example.com", fullName: "John Doe");
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.Handle(new GetCurrentUserQuery(1), CancellationToken.None);

        // Assert
        result.Id.ShouldBe(1);
        result.Email.ShouldBe("john@example.com");
        result.FullName.ShouldBe("John Doe");
        result.Avatar.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepository.GetByIdAsync(99, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => _sut.Handle(new GetCurrentUserQuery(99), CancellationToken.None);

        // Assert
        await Should.ThrowAsync<NotFoundException>(act)
            .ContinueWith(t => t.Result.Message.ShouldContain("99"));
    }
}
