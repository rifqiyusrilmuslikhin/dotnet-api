using DotnetApi.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DotnetApi.Api.Adapters;

/// <summary>
/// Adapts ASP.NET Core <see cref="IFormFile"/> to the domain <see cref="IUploadedFile"/> abstraction,
/// keeping Application and Domain layers free of ASP.NET Core dependencies.
/// </summary>
public sealed class FormFileAdapter : IUploadedFile
{
    private readonly IFormFile _formFile;

    public FormFileAdapter(IFormFile formFile)
    {
        _formFile = formFile;
    }

    public string FileName => _formFile.FileName;
    public string ContentType => _formFile.ContentType;
    public long Length => _formFile.Length;
    public Stream OpenReadStream() => _formFile.OpenReadStream();
}
