using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spizarnia.Application.Venues;

namespace Spizarnia.Api.Controllers;

[ApiController]
[Route("api/venues")]
[Authorize]
public class VenuesController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] double lat, [FromQuery] double lon, [FromQuery] string? q, CancellationToken ct)
    {
        var result = await mediator.Send(new SearchVenuesQuery(lat, lon, q), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{venueId}/menu")]
    public async Task<IActionResult> GetMenu(string venueId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetVenueMenuQuery(venueId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }
}
