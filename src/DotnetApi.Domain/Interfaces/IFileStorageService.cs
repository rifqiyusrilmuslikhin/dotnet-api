namespace DotnetApi.Domain.Interfaces;

/// <summary>
/// Contract for storing and deleting uploaded files
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file stream and returns the relative path to the saved file
    /// </summary>
    Task<string> SaveAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file by its relative path. Does nothing if the file does not exist.
    /// </summary>
    Task DeleteAsync(string? filePath, CancellationToken cancellationToken = default);
}
