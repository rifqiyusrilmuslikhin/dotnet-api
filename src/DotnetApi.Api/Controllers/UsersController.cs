using DotnetApi.Application.Common.Models;
using DotnetApi.Application.Features.Users.Commands.ChangePassword;
using DotnetApi.Application.Features.Users.Commands.UpdateUser;
using DotnetApi.Application.Features.Users.Queries.GetAllUsers;
using DotnetApi.Application.Features.Users.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Api.Controllers;

/// <summary>
/// Handles user management operations
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

    /// <summary>
    /// Retrieves all registered users
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all users</returns>
    /// <response code="200">Returns the list of users</response>
    /// <response code="401">Unauthorized — valid JWT token required</response>
    [HttpGet(Name = "GetAllUsers")]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a user by their ID
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="401">Unauthorized — valid JWT token required</response>
    /// <response code="404">User not found</response>
    [HttpGet("{id:int}", Name = "GetUserById")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates a user's profile information
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <param name="command">Updated profile data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user details</returns>
    /// <response code="200">User updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Unauthorized — valid JWT token required</response>
    /// <response code="404">User not found</response>
    [HttpPut("{id:int}", Name = "UpdateUser")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { Id = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Changes a user's password
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <param name="command">Current and new password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">Password changed successfully</response>
    /// <response code="400">Validation error or current password incorrect</response>
    /// <response code="401">Unauthorized — valid JWT token required</response>
    /// <response code="404">User not found</response>
    [HttpPatch("{id:int}/password", Name = "ChangePassword")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
        int id,
        [FromBody] ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command with { Id = id }, cancellationToken);
        return NoContent();
    }
}
