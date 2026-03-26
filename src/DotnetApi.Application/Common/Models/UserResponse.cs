namespace DotnetApi.Application.Common.Models;

public record UserResponse(
    int Id,
    string Email,
    string FullName,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
