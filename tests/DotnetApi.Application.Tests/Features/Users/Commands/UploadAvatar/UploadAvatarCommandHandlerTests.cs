using DotnetApi.Application.Features.Users.Commands.UploadAvatar;
using DotnetApi.Domain.Exceptions;
using DotnetApi.Domain.Interfaces;
using DotnetApi.Domain.Tests.Helpers;
using Shouldly;
using NSubstitute;
using User = DotnetApi.Domain.Entities.User;

namespace DotnetApi.Application.Tests.Features.Users.Commands.UploadAvatar;

public class UploadAvatarCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IFileStorageService _fileStorageService = Substitute.For<IFileStorageService>();
    private readonly UploadAvatarCommandHandler _sut;

    public UploadAvatarCommandHandlerTests()
    {
        _sut = new UploadAvatarCommandHandler(_userRepository, _fileStorageService);
    }

    [Fact]
    public async Task Handle_WithValidFile_ShouldSaveAndReturnUpdatedUser()
    {
        // Arrange
        var user = UserFactory.Create(id: 1, email: "john@example.com");
        var file = new FakeUploadedFile("avatar.jpg", "image/jpeg", new byte[512]);
        var command = new UploadAvatarCommand(UserId: 1, File: file);

        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);
        _fileStorageService.SaveAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("/uploads/avatars/1_abc.jpg");
        _userRepository.UpdateAsync(user, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Avatar.ShouldBe("/uploads/avatars/1_abc.jpg");
        await _fileStorageService.Received(1)
            .SaveAsync(Arg.Any<Stream>(), Arg.Is<string>(s => s.StartsWith("avatars/")), "image/jpeg", Arg.Any<CancellationToken>());
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserHasExistingAvatar_ShouldDeleteOldAvatarFirst()
    {
        // Arrange
        var user = UserFactory.Create(id: 1, avatar: "/uploads/avatars/old.jpg");
        var file = new FakeUploadedFile("new.jpg", "image/jpeg", new byte[512]);
        var command = new UploadAvatarCommand(UserId: 1, File: file);

        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);
        _fileStorageService.SaveAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("/uploads/avatars/1_new.jpg");
        _userRepository.UpdateAsync(user, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert — delete harus dipanggil dengan path avatar lama
        await _fileStorageService.Received(1)
            .DeleteAsync("/uploads/avatars/old.jpg", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new UploadAvatarCommand(UserId: 99, File: new FakeUploadedFile());

        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<NotFoundException>(act);
    }
}
