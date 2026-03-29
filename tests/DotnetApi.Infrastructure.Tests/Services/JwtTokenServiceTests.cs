using DotnetApi.Domain.Tests.Helpers;
using DotnetApi.Infrastructure.Services;
using Shouldly;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace DotnetApi.Infrastructure.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"]        = "super_secret_key_min_32_chars_long!!",
                ["Jwt:Issuer"]           = "TestIssuer",
                ["Jwt:Audience"]         = "TestAudience",
                ["Jwt:ExpiresInSeconds"] = "3600"
            })
            .Build();

        _sut = new JwtTokenService(config);
    }

    [Fact]
    public void GenerateToken_ShouldReturnNonEmptyString()
    {
        var user = UserFactory.Create(id: 1, email: "john@example.com", fullName: "John Doe");

        var token = _sut.GenerateToken(user);

        token.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_ShouldContainCorrectClaims()
    {
        var user = UserFactory.Create(id: 1, email: "john@example.com", fullName: "John Doe");

        var token = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "1");
        jwt.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "john@example.com");
        jwt.Issuer.ShouldBe("TestIssuer");
        jwt.Audiences.ShouldContain("TestAudience");
    }

    [Fact]
    public void GenerateToken_ShouldExpireAccordingToConfig()
    {
        var user = UserFactory.Create(id: 1, email: "john@example.com", fullName: "John Doe");

        var token = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var expiresIn = jwt.ValidTo - DateTime.UtcNow;
        expiresIn.ShouldBeInRange(TimeSpan.FromSeconds(3590), TimeSpan.FromSeconds(3610));
    }
}
