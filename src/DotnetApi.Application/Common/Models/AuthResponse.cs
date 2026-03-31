namespace DotnetApi.Application.Common.Models;

public record AuthResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string RefreshToken,
    int UserId,
    string Email,
    string FullName
);
