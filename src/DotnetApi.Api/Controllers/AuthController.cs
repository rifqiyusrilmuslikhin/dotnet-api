using DotnetApi.Application.Common.Models;
using DotnetApi.Application.Features.Auth.Commands.Login;
using DotnetApi.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Api.Controllers;

/// <summary>
/// Handles authentication operations: register and login
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="command">Registration details including name, email, and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication token and user info</returns>
    /// <response code="201">User registered successfully, returns JWT token</response>
    /// <response code="400">Validation error or email already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Register), result);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="command">Login credentials: email and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication token and user info</returns>
    /// <response code="200">Login successful, returns JWT token</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Invalid email or password</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
