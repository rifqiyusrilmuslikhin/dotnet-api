using DotnetApi.Domain.Interfaces;
using DotnetApi.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DotnetApi.Infrastructure.Services;

/// <summary>
/// Stores uploaded files on the local disk under the configured upload path
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private readonly string _baseUrl;

    public LocalFileStorageService(IOptions<FileStorageOptions> options, IHostEnvironment environment)
    {
        var opts = options.Value;
        _rootPath = Path.IsPathRooted(opts.UploadPath)
            ? opts.UploadPath
            : Path.Combine(environment.ContentRootPath, opts.UploadPath);
        _baseUrl = opts.BaseUrl.TrimEnd('/');
    }

    /// <inheritdoc/>
    public async Task<string> SaveAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, fileName);

        // Ensure subdirectory exists (e.g. uploads/avatars/)
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);

        await using var fileOutput = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(fileOutput, cancellationToken);

        // Return the public relative URL, e.g. "/uploads/avatars/1_abc.jpg"
        return $"{_baseUrl}/{fileName.Replace('\\', '/')}";
    }

    /// <inheritdoc/>
    public Task DeleteAsync(string? filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Task.CompletedTask;

        // Extract local path from public URL
        var relativePath = filePath.Replace(_baseUrl, string.Empty).TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_rootPath, relativePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
