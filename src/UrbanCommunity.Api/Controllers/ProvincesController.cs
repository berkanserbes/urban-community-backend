using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Features.Provinces.Queries.GetDistrictsByProvince;
using UrbanCommunity.Application.Features.Provinces.Queries.GetProvinceById;
using UrbanCommunity.Application.Features.Provinces.Queries.GetProvinces;
using UrbanCommunity.Application.Features.Users.Queries.GetUsersByProvince;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/provinces")]
public class ProvincesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns all provinces ordered alphabetically.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProvinces(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProvincesQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns the specified province.
    /// </summary>
    [HttpGet("{provinceId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid provinceId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProvinceByIdQuery(provinceId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns all districts belonging to the specified province, ordered alphabetically.
    /// </summary>
    [HttpGet("{provinceId:guid}/districts")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDistrictsByProvince(Guid provinceId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDistrictsByProvinceQuery(provinceId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns all active users living in the specified province.
    /// </summary>
    [HttpGet("{provinceId:guid}/users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsersByProvince(Guid provinceId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUsersByProvinceQuery(provinceId), cancellationToken);
        return Ok(result);
    }
}
