using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Features.Announcements.Commands.ActivateAllAnnouncements;
using UrbanCommunity.Application.Features.Announcements.Commands.ActivateAnnouncement;
using UrbanCommunity.Application.Features.Announcements.Commands.CreateAnnouncement;
using UrbanCommunity.Application.Features.Announcements.Commands.DeactivateAllAnnouncements;
using UrbanCommunity.Application.Features.Announcements.Commands.DeactivateAnnouncement;
using UrbanCommunity.Application.Features.Announcements.Commands.DeleteAllAnnouncements;
using UrbanCommunity.Application.Features.Announcements.Commands.DeleteAnnouncement;
using UrbanCommunity.Application.Features.Announcements.Commands.UpdateAnnouncement;
using UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncementById;
using UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncements;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/announcements")]
[Authorize]
public class AnnouncementsController(IMediator mediator) : ApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? provinceId,
        [FromQuery] Guid? districtId,
        [FromQuery] Guid? neighborhoodId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAnnouncementsQuery(provinceId, districtId, neighborhoodId, isActive), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAnnouncementByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> Create([FromBody] CreateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new CreateAnnouncementCommand(
            request.Title,
            request.Content,
            request.Scope,
            request.ProvinceId,
            request.DistrictId,
            request.NeighborhoodId,
            request.ExpiresAt,
            userId);

        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateAnnouncementCommand(id, request.Title, request.Content, request.ExpiresAt), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeactivateAnnouncementCommand(id), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> Activate(Guid id, [FromBody] ActivateAnnouncementRequest? request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ActivateAnnouncementCommand(id, request?.ExpiresAt), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteAnnouncementCommand(id), cancellationToken);
        return Ok(result);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteAllAnnouncementsCommand(), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("activate-all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActivateAll([FromBody] ActivateAllAnnouncementsRequest? request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ActivateAllAnnouncementsCommand(request?.ExpiresAt), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("deactivate-all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeactivateAllAnnouncementsCommand(), cancellationToken);
        return Ok(result);
    }

}

public sealed record CreateAnnouncementRequest(
    string Title,
    string Content,
    AnnouncementScope Scope,
    Guid? ProvinceId,
    Guid? DistrictId,
    Guid? NeighborhoodId,
    DateTime? ExpiresAt);

public sealed record UpdateAnnouncementRequest(string Title, string Content, DateTime? ExpiresAt);
public sealed record ActivateAnnouncementRequest(DateTime? ExpiresAt);
public sealed record ActivateAllAnnouncementsRequest(DateTime? ExpiresAt);

