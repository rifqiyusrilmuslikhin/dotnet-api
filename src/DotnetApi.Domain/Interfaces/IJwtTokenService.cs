using DotnetApi.Domain.Entities;

namespace DotnetApi.Domain.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    int RefreshTokenExpiresInDays { get; }
}
