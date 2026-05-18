using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Features.Provinces.Queries.GetDistrictsByProvince;
using UrbanCommunity.Application.Features.Provinces.Queries.GetProvinces;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProvincesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns all provinces ordered alphabetically.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProvincesQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns all districts belonging to the specified province, ordered alphabetically.
    /// </summary>
    [HttpGet("{provinceId:guid}/districts")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDistricts(Guid provinceId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDistrictsByProvinceQuery(provinceId), cancellationToken);
        return Ok(result);
    }
}
