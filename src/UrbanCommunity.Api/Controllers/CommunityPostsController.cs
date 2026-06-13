using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Api.Constants;
using UrbanCommunity.Application.Features.CommunityPosts.Commands.ChangeCommunityPostStatus;
using UrbanCommunity.Application.Features.CommunityPosts.Commands.CreateCommunityPost;
using UrbanCommunity.Application.Features.CommunityPosts.Commands.DeleteCommunityPost;
using UrbanCommunity.Application.Features.CommunityPosts.Commands.UpdateCommunityPost;
using UrbanCommunity.Application.Features.CommunityPosts.Queries.GetAllowedCommunityPostStatusTransitions;
using UrbanCommunity.Application.Features.CommunityPosts.Queries.GetCommunityPostById;
using UrbanCommunity.Application.Features.CommunityPosts.Queries.GetCommunityPosts;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/community-posts")]
[Authorize]
public class CommunityPostsController(IMediator mediator) : ApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] CommunityPostScope? scope,
        [FromQuery] Guid? provinceId,
        [FromQuery] Guid? districtId,
        [FromQuery] Guid? neighborhoodId,
        [FromQuery] CommunityPostStatus? status,
        [FromQuery] PostType? type,
        [FromQuery] Guid? userId,
        CancellationToken cancellationToken)
    {
        var requestingUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await mediator.Send(
            new GetCommunityPostsQuery(requestingUserId, isAdmin, scope, provinceId, districtId, neighborhoodId, status, type, userId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var requestingUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await mediator.Send(new GetCommunityPostByIdQuery(id, requestingUserId, isAdmin), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/allowed-status-transitions")]
    public async Task<IActionResult> GetAllowedStatusTransitions(Guid id, CancellationToken cancellationToken)
    {
        var requestingUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await mediator.Send(
            new GetAllowedCommunityPostStatusTransitionsQuery(id, requestingUserId, isAdmin),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [RequestSizeLimit(ApiConstants.MaxUploadSize)]
    public async Task<IActionResult> Create([FromForm] CreateCommunityPostRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new CreateCommunityPostCommand(
            request.Type,
            request.Title,
            request.Content,
            userId,
            request.Scope,
            request.ProvinceId,
            request.DistrictId,
            request.NeighborhoodId,
            request.Media);

        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [RequestSizeLimit(ApiConstants.MaxUploadSize)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateCommunityPostRequest request, CancellationToken cancellationToken)
    {
        var requestingUserId = GetCurrentUserId();
        var result = await mediator.Send(new UpdateCommunityPostCommand(
            id,
            requestingUserId,
            request.Title,
            request.Content,
            request.KeepMediaIds,
            request.Media), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var requestingUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await mediator.Send(new DeleteCommunityPostCommand(id, requestingUserId, isAdmin), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeCommunityPostStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ChangeCommunityPostStatusCommand(
                id,
                GetCurrentUserId(),
                User.IsInRole("Admin"),
                request.NewStatus),
            cancellationToken);
        return Ok(result);
    }
}

public sealed class CreateCommunityPostRequest
{
    public PostType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public CommunityPostScope Scope { get; init; } = CommunityPostScope.Neighborhood;
    public Guid? ProvinceId { get; init; }
    public Guid? DistrictId { get; init; }
    public Guid? NeighborhoodId { get; init; }
    public IReadOnlyList<IFormFile>? Media { get; init; }
}

public sealed class UpdateCommunityPostRequest
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public IReadOnlyList<Guid>? KeepMediaIds { get; init; }
    public IReadOnlyList<IFormFile>? Media { get; init; }
}
public sealed record ChangeCommunityPostStatusRequest(CommunityPostStatus NewStatus);
