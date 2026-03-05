using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spizarnia.Application.Profile;
using System.Security.Claims;

namespace Spizarnia.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController(ISender mediator) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!);

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await mediator.Send(new GetProfileQuery(CurrentUserId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateProfileCommand(
            CurrentUserId, req.Name, req.Language,
            req.DietaryRestrictions, req.Allergies, req.Goals), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPut("family")]
    public async Task<IActionResult> UpdateFamily([FromBody] UpdateFamilyRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateFamilyCommand(CurrentUserId, req.Members), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}

public record UpdateProfileRequest(
    string? Name, string? Language,
    List<string>? DietaryRestrictions, List<string>? Allergies, List<string>? Goals);

public record UpdateFamilyRequest(List<FamilyMemberDto> Members);
