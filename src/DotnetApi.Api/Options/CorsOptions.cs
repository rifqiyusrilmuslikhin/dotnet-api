namespace DotnetApi.Api.Options;

public class CorsOptions
{
    public const string SectionName = "Cors";

    /// <summary>
    /// List of allowed origins for CORS policy.
    /// Example: ["http://localhost:4200", "https://your-frontend.com"]
    /// </summary>
    public string[] AllowedOrigins { get; init; } = [];
}
