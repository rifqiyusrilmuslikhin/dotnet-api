namespace DotnetApi.Infrastructure.Options;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Root directory where uploaded files are stored, relative to the application content root.
    /// Default: "uploads"
    /// </summary>
    public string UploadPath { get; init; } = "uploads";

    /// <summary>
    /// Base URL prefix used to build a public URL for stored files.
    /// Example: "/uploads" or "https://cdn.example.com"
    /// </summary>
    public string BaseUrl { get; init; } = "/uploads";
}
