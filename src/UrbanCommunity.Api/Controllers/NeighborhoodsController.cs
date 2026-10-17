using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodById;
using UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoods;
using UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodLocation;
using UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodsByDistrict;
using UrbanCommunity.Application.Features.Users.Queries.GetUsersByNeighborhood;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/neighborhoods")]
public class NeighborhoodsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetNeighborhoods(
        [FromQuery] Guid? provinceId,
        [FromQuery] Guid? districtId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNeighborhoodsQuery(provinceId, districtId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns the specified neighborhood with its district and province info.
    /// </summary>
    [HttpGet("{neighborhoodId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid neighborhoodId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNeighborhoodByIdQuery(neighborhoodId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns all neighborhoods belonging to the specified district, ordered alphabetically.
    /// </summary>
    [HttpGet("/api/districts/{districtId:guid}/neighborhoods")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNeighborhoodsByDistrict(Guid districtId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNeighborhoodsByDistrictQuery(districtId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns the province, district and neighborhood info for the given neighborhood id.
    /// </summary>
    [HttpGet("{neighborhoodId:guid}/location")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNeighborhoodLocation(Guid neighborhoodId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNeighborhoodLocationQuery(neighborhoodId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns all active users living in the specified neighborhood.
    /// </summary>
    [HttpGet("{neighborhoodId:guid}/users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Result<PagedResult<GetUsersByNeighborhoodQueryResponse>>), 200)]
    public async Task<IActionResult> GetUsersByNeighborhood(
        [FromQuery] PaginationRequest pagination,
        Guid neighborhoodId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetUsersByNeighborhoodQuery(neighborhoodId)
            {
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize
            },
            cancellationToken);
        return Ok(result);
    }
}
