using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Features.Suggestions.Commands.ChangeSuggestionStatus;
using UrbanCommunity.Application.Features.Suggestions.Commands.CreateSuggestion;
using UrbanCommunity.Application.Features.Suggestions.Commands.DeleteSuggestion;
using UrbanCommunity.Application.Features.Suggestions.Commands.UpdateSuggestion;
using UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestionById;
using UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestions;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuggestionsController : ApiController
{
    private readonly IMediator _mediator;

    public SuggestionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SuggestionStatus? status, [FromQuery] Guid? userId)
    {
        var result = await _mediator.Send(new GetSuggestionsQuery(status, userId));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetSuggestionByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSuggestionCommand command)
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(command with { UserId = userId });
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSuggestionRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new UpdateSuggestionCommand(id, userId, request.Title, request.Content));
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await _mediator.Send(new DeleteSuggestionCommand(id, userId, isAdmin));
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeSuggestionStatusRequest request)
    {
        var result = await _mediator.Send(new ChangeSuggestionStatusCommand(id, request.NewStatus));
        return Ok(result);
    }
}

public sealed record ChangeSuggestionStatusRequest(SuggestionStatus NewStatus);
public sealed record UpdateSuggestionRequest(string Title, string Content);
