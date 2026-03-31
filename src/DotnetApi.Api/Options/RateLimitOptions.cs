namespace DotnetApi.Api.Options;

public class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    /// <summary>
    /// Maximum number of requests allowed per window.
    /// Default: 100
    /// </summary>
    public int PermitLimit { get; init; } = 100;

    /// <summary>
    /// Time window in seconds for the rate limit.
    /// Default: 60 (1 minute)
    /// </summary>
    public int WindowSeconds { get; init; } = 60;
}
