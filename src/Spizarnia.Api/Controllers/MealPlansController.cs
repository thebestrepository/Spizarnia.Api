using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spizarnia.Application.MealPlans;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Spizarnia.Api.Controllers;

[ApiController]
[Route("api/meal-plans")]
[Authorize]
public class MealPlansController(ISender mediator) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(CancellationToken ct)
    {
        var result = await mediator.Send(new GenerateMealPlanCommand(CurrentUserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(CancellationToken ct)
    {
        var result = await mediator.Send(new GetCurrentMealPlanQuery(CurrentUserId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMealPlanByIdQuery(CurrentUserId, id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMealPlanRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateMealPlanCommand(CurrentUserId, id, req.Meals), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}

public record UpdateMealPlanRequest(List<MealDto> Meals);
