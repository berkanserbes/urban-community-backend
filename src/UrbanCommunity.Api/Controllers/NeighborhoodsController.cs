using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoods;
using UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodsByDistrict;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NeighborhoodsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? city, [FromQuery] string? district, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNeighborhoodsQuery(city, district), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns all neighborhoods belonging to the specified district, ordered alphabetically.
    /// </summary>
    [HttpGet("by-district/{districtId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByDistrict(Guid districtId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNeighborhoodsByDistrictQuery(districtId), cancellationToken);
        return Ok(result);
    }
}
