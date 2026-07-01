using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Features.Suggestions.Commands.ChangeSuggestionStatus;
using UrbanCommunity.Application.Features.Suggestions.Commands.CreateSuggestion;
using UrbanCommunity.Application.Features.Suggestions.Commands.DeleteSuggestion;
using UrbanCommunity.Application.Features.Suggestions.Commands.UpdateSuggestionOfficialResponse;
using UrbanCommunity.Application.Features.Suggestions.Commands.UpdateSuggestion;
using UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestionById;
using UrbanCommunity.Application.Features.Suggestions.Queries.GetAllowedSuggestionStatusTransitions;
using UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestions;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/suggestions")]
[Authorize]
public class SuggestionsController(IMediator mediator) : ApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] SuggestionStatus? status,
        [FromQuery] Guid? userId,
        [FromQuery] Guid? neighborhoodId,
        [FromQuery] Guid? districtId,
        [FromQuery] Guid? provinceId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetSuggestionsQuery(status, userId, neighborhoodId, districtId, provinceId, GetCurrentUserId()),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSuggestionByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/allowed-status-transitions")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> GetAllowedStatusTransitions(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAllowedSuggestionStatusTransitionsQuery(id, GetCurrentUserId()),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSuggestionRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new CreateSuggestionCommand(request.Title, request.Content, userId), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSuggestionRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new UpdateSuggestionCommand(id, userId, request.Title, request.Content), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await mediator.Send(new DeleteSuggestionCommand(id, userId, isAdmin), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeSuggestionStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ChangeSuggestionStatusCommand(
                id,
                GetCurrentUserId(),
                request.NewStatus),
            cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/official-response")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> UpdateOfficialResponse(
        Guid id,
        [FromBody] UpdateSuggestionOfficialResponseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateSuggestionOfficialResponseCommand(
                id,
                GetCurrentUserId(),
                request.OfficialResponse),
            cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateSuggestionRequest(string Title, string Content);
public sealed record ChangeSuggestionStatusRequest(SuggestionStatus NewStatus);
public sealed record UpdateSuggestionRequest(string Title, string Content);
public sealed record UpdateSuggestionOfficialResponseRequest(string? OfficialResponse);
