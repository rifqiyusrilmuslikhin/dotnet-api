using DotnetApi.Infrastructure.Options;
using DotnetApi.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace DotnetApi.Infrastructure.Tests.Services;

public class GoogleTokenValidatorTests
{
    private readonly GoogleTokenValidator _sut;

    public GoogleTokenValidatorTests()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new GoogleAuthOptions
        {
            ClientId = "test-client-id.apps.googleusercontent.com"
        });
        var logger = Substitute.For<ILogger<GoogleTokenValidator>>();

        _sut = new GoogleTokenValidator(options, logger);
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidToken_ShouldReturnNull()
    {
        // An obviously invalid token should be caught by Google.Apis.Auth
        var result = await _sut.ValidateAsync("this-is-not-a-valid-jwt-token");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyToken_ShouldReturnNull()
    {
        var result = await _sut.ValidateAsync("");

        result.ShouldBeNull();
    }
}
