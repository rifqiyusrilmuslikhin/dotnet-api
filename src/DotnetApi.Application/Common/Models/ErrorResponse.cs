using System.Text.Json.Serialization;

namespace DotnetApi.Application.Common.Models;

/// <summary>
/// Standardized error response returned to clients
/// </summary>
public record ErrorResponse
{
    public int Status { get; init; }
    public string Title { get; init; } = null!;
    [JsonPropertyName("msg")]
    public string? Detail { get; init; }
    public string? TraceId { get; init; }
    public IDictionary<string, string[]>? Errors { get; init; }
}
