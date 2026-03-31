namespace DotnetApi.Domain.Interfaces;

/// <summary>
/// Validates a Google ID token and returns the user's Google profile info.
/// </summary>
public interface IGoogleTokenValidator
{
    /// <summary>
    /// Validates the Google ID token and returns the payload if valid.
    /// </summary>
    /// <param name="idToken">The Google ID token received from the frontend</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Google user info if the token is valid; null otherwise</returns>
    Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the essential user information extracted from a Google ID token.
/// </summary>
public record GoogleUserInfo(
    string Subject,
    string Email,
    string FullName
);
