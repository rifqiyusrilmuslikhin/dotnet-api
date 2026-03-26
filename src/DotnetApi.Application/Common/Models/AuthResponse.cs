namespace DotnetApi.Application.Common.Models;

public record AuthResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    int UserId,
    string Email,
    string FullName
);
