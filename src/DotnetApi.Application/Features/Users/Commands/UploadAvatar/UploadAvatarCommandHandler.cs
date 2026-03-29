using DotnetApi.Application.Common.Models;
using DotnetApi.Domain.Exceptions;
using DotnetApi.Domain.Interfaces;
using MediatR;

namespace DotnetApi.Application.Features.Users.Commands.UploadAvatar;

/// <summary>
/// Handles uploading and replacing the avatar for the currently authenticated user
/// </summary>
public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;

    public UploadAvatarCommandHandler(
        IUserRepository userRepository,
        IFileStorageService fileStorageService)
    {
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
    }

    /// <inheritdoc/>
    public async Task<UserResponse> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        // Delete old avatar if exists
        await _fileStorageService.DeleteAsync(user.Avatar, cancellationToken);

        // Generate a unique filename to prevent collisions and path traversal
        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        var fileName = $"avatars/{user.Id}_{Guid.NewGuid()}{extension}";

        // Persist the new file
        using var stream = request.File.OpenReadStream();
        var savedPath = await _fileStorageService.SaveAsync(stream, fileName, request.File.ContentType, cancellationToken);

        // Update domain entity
        user.UpdateAvatar(savedPath);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new UserResponse(
            Id: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt,
            Avatar: user.Avatar
        );
    }
}
