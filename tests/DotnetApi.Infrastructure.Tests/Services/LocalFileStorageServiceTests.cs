using DotnetApi.Domain.Tests.Helpers;
using DotnetApi.Infrastructure.Options;
using DotnetApi.Infrastructure.Services;
using Shouldly;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DotnetApi.Infrastructure.Tests.Services;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly LocalFileStorageService _sut;

    public LocalFileStorageServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "dotnet_tests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempRoot);

        var options = Microsoft.Extensions.Options.Options.Create(new FileStorageOptions
        {
            UploadPath = _tempRoot,
            BaseUrl = "/uploads"
        });

        var env = Substitute.For<IHostEnvironment>();
        env.ContentRootPath.Returns(_tempRoot);

        _sut = new LocalFileStorageService(options, env);
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateFileAndReturnPublicUrl()
    {
        var content = "fake image bytes"u8.ToArray();
        using var stream = new MemoryStream(content);

        var url = await _sut.SaveAsync(stream, "avatars/test.jpg", "image/jpeg");

        url.ShouldStartWith("/uploads/avatars/");
        url.ShouldEndWith("test.jpg");

        var filePath = Path.Combine(_tempRoot, "avatars", "test.jpg");
        File.Exists(filePath).ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingFile_ShouldRemoveFile()
    {
        var subDir = Path.Combine(_tempRoot, "avatars");
        Directory.CreateDirectory(subDir);
        var filePath = Path.Combine(subDir, "todelete.jpg");
        await File.WriteAllBytesAsync(filePath, [1, 2, 3]);

        await _sut.DeleteAsync("/uploads/avatars/todelete.jpg");

        File.Exists(filePath).ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithNullPath_ShouldNotThrow()
    {
        var act = async () => await _sut.DeleteAsync(null);

        await Should.NotThrowAsync(act);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentFile_ShouldNotThrow()
    {
        var act = async () => await _sut.DeleteAsync("/uploads/avatars/ghost.jpg");

        await Should.NotThrowAsync(act);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }
}
