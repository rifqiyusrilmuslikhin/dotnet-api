namespace DotnetApi.Infrastructure.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; init; } = null!;
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public int ExpiresInSeconds { get; init; } = 3600;
}
