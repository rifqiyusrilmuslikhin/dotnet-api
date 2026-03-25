using DotnetApi.Application.Features.HealthCheck.Queries.GetHealthCheck;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    private readonly IMediator _mediator;

    public HealthCheckController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get the health status of the application
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetHealthCheckQueryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetHealthCheckQuery(), cancellationToken);

        return result.Status == "Unhealthy"
            ? StatusCode(503, result)
            : Ok(result);
    }
}
