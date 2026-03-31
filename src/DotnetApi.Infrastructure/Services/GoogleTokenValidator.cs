using DotnetApi.Domain.Interfaces;
using DotnetApi.Infrastructure.Options;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetApi.Infrastructure.Services;

/// <summary>
/// Validates Google ID tokens using Google.Apis.Auth and returns user info.
/// </summary>
public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleAuthOptions _options;
    private readonly ILogger<GoogleTokenValidator> _logger;

    public GoogleTokenValidator(
        IOptions<GoogleAuthOptions> options,
        ILogger<GoogleTokenValidator> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            _logger.LogWarning("Empty Google ID token received");
            return null;
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_options.ClientId]
            });

            if (string.IsNullOrWhiteSpace(payload.Subject) || string.IsNullOrWhiteSpace(payload.Email))
            {
                _logger.LogWarning("Google token payload missing required fields (sub or email)");
                return null;
            }

            return new GoogleUserInfo(
                Subject: payload.Subject,
                Email: payload.Email.Trim().ToLowerInvariant(),
                FullName: payload.Name?.Trim() ?? payload.Email
            );
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google ID token received");
            return null;
        }
    }
}
