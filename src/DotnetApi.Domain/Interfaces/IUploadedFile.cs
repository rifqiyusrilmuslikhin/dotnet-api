namespace DotnetApi.Domain.Interfaces;

/// <summary>
/// Abstraction for an uploaded file, decoupling Application layer from ASP.NET Core
/// </summary>
public interface IUploadedFile
{
    /// <summary>Original file name as provided by the client</summary>
    string FileName { get; }

    /// <summary>MIME content type (e.g. "image/jpeg")</summary>
    string ContentType { get; }

    /// <summary>File size in bytes</summary>
    long Length { get; }

    /// <summary>Opens a readable stream of the file content</summary>
    Stream OpenReadStream();
}
