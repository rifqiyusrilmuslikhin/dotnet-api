using DotnetApi.Application.Common.Models;
using DotnetApi.Application.Features.Auth.Commands.GoogleLogin;
using DotnetApi.Application.Features.Auth.Commands.Login;
using DotnetApi.Application.Features.Auth.Commands.RefreshToken;
using DotnetApi.Application.Features.Auth.Commands.Register;
using DotnetApi.Application.Features.Auth.Commands.RevokeToken;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Api.Controllers;

/// <summary>
/// Handles authentication operations: register, login, Google OAuth2, token refresh, and token revocation
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
    [HttpPost("register", Name = "Register")]
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
    /// <response code="400">Validation error or invalid credentials</response>
    [HttpPost("login", Name = "Login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates or registers a user via Google OAuth2 ID token
    /// </summary>
    /// <param name="command">Google ID token received from the frontend</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication token and user info</returns>
    /// <response code="200">Google login successful, returns JWT token</response>
    /// <response code="400">Invalid Google ID token or validation error</response>
    [HttpPost("google", Name = "GoogleLogin")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GoogleLogin(
        [FromBody] GoogleLoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Refreshes an access token using a valid refresh token (token rotation)
    /// </summary>
    /// <param name="command">The refresh token to exchange for a new token pair</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New access token and refresh token</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="400">Invalid, expired, or revoked refresh token</response>
    [HttpPost("refresh", Name = "RefreshToken")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Revokes a refresh token so it can no longer be used
    /// </summary>
    /// <param name="command">The refresh token to revoke</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Refresh token revoked successfully</response>
    /// <response code="400">Invalid or already revoked refresh token</response>
    [HttpPost("revoke", Name = "RevokeToken")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Revoke(
        [FromBody] RevokeTokenCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
