using DotnetApi.Application.Common.Models;
using DotnetApi.Domain.Interfaces;
using MediatR;

namespace DotnetApi.Application.Features.Users.Commands.UploadAvatar;

/// <summary>
/// Command to upload or replace the avatar of the currently authenticated user
/// </summary>
public record UploadAvatarCommand(
    int UserId,
    IUploadedFile File
) : IRequest<UserResponse>;
