using DotnetApi.Domain.Interfaces;

namespace DotnetApi.Domain.Tests.Helpers;

/// <summary>
/// Fake implementation of IUploadedFile for testing purposes, without a dependency on IFormFile.
/// </summary>
public sealed class FakeUploadedFile : IUploadedFile
{
    private readonly byte[] _content;

    public FakeUploadedFile(
        string fileName = "avatar.jpg",
        string contentType = "image/jpeg",
        byte[]? content = null)
    {
        FileName = fileName;
        ContentType = contentType;
        _content = content ?? new byte[1024]; // 1KB default
    }

    public string FileName { get; }
    public string ContentType { get; }
    public long Length => _content.Length;
    public Stream OpenReadStream() => new MemoryStream(_content);
}
