using DotnetApi.Api.Adapters;
using DotnetApi.Application.Common.Models;
using DotnetApi.Application.Features.Users.Commands.ChangeCurrentUserPassword;
using DotnetApi.Application.Features.Users.Commands.UpdateCurrentUser;
using DotnetApi.Application.Features.Users.Commands.UploadAvatar;
using DotnetApi.Application.Features.Users.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DotnetApi.Api.Controllers;

/// <summary>
/// Handles operations for the currently authenticated user
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User identity could not be determined."));

    /// <summary>
    /// Retrieves the currently authenticated user profile
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current user profile</returns>
    /// <response code="200">Returns the current user</response>
    /// <response code="401">Unauthorized - valid JWT token required</response>
    [HttpGet("me", Name = "GetCurrentUser")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(GetCurrentUserId()), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the currently authenticated user profile
    /// </summary>
    /// <param name="command">Updated profile data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user profile</returns>
    /// <response code="200">Profile updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Unauthorized - valid JWT token required</response>
    [HttpPut("me", Name = "UpdateCurrentUser")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateCurrentUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { UserId = GetCurrentUserId() }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Changes the currently authenticated user password
    /// </summary>
    /// <param name="command">Current and new password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">Password changed successfully</response>
    /// <response code="400">Validation error or current password incorrect</response>
    /// <response code="401">Unauthorized - valid JWT token required</response>
    [HttpPatch("me/password", Name = "ChangeCurrentUserPassword")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangeCurrentUserPasswordCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command with { UserId = GetCurrentUserId() }, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Uploads or replaces the avatar image for the currently authenticated user
    /// </summary>
    /// <param name="file">Image file (JPEG, PNG, or WebP, max 2 MB)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user profile with new avatar URL</returns>
    /// <response code="200">Avatar uploaded successfully</response>
    /// <response code="400">Invalid file type, size, or missing file</response>
    /// <response code="401">Unauthorized - valid JWT token required</response>
    [HttpPost("me/avatar", Name = "UploadAvatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadAvatar(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var command = new UploadAvatarCommand(GetCurrentUserId(), new FormFileAdapter(file));
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
