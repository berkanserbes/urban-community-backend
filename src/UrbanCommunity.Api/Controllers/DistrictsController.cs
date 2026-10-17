using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.Districts.Queries.GetDistrictById;
using UrbanCommunity.Application.Features.Users.Queries.GetUsersByDistrict;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/districts")]
public class DistrictsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns the specified district.
    /// </summary>
    [HttpGet("{districtId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid districtId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDistrictByIdQuery(districtId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns all active users living in the specified district.
    /// </summary>
    [HttpGet("{districtId:guid}/users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Result<PagedResult<GetUsersByDistrictQueryResponse>>), 200)]
    public async Task<IActionResult> GetUsersByDistrict(
        [FromQuery] PaginationRequest pagination,
        Guid districtId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetUsersByDistrictQuery(districtId)
            {
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize
            },
            cancellationToken);
        return Ok(result);
    }
}
