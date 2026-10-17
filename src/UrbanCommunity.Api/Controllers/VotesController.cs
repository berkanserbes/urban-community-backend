using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.Votes.Commands.RemoveSuggestionVote;
using UrbanCommunity.Application.Features.Votes.Commands.SetSuggestionVote;
using UrbanCommunity.Application.Features.Votes.Queries.GetSuggestionVoteSummary;
using UrbanCommunity.Application.Features.Votes.Queries.GetSuggestionVoters;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/suggestions/{suggestionId:guid}/votes")]
public class VotesController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Creates or updates the authenticated user's vote for the specified suggestion.
    /// </summary>
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> SetSuggestionVote(
        Guid suggestionId,
        [FromBody] SetSuggestionVoteRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new SetSuggestionVoteCommand(suggestionId, userId, request.IsUpvote), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Removes the authenticated user's vote from the specified suggestion.
    /// </summary>
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> RemoveSuggestionVote(Guid suggestionId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new RemoveSuggestionVoteCommand(suggestionId, userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns vote counts and the current user's vote if the request is authenticated.
    /// </summary>
    [HttpGet("summary")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSuggestionVoteSummary(Guid suggestionId, CancellationToken cancellationToken)
    {
        var userId = TryGetCurrentUserId();
        var result = await mediator.Send(new GetSuggestionVoteSummaryQuery(suggestionId, userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns users who voted for the specified suggestion.
    /// </summary>
    [HttpGet("voters")]
    [Authorize(Roles = "Admin,Official")]
    [ProducesResponseType(typeof(Result<PagedResult<VoterItem>>), 200)]
    public async Task<IActionResult> GetSuggestionVoters(
        [FromQuery] PaginationRequest pagination,
        [FromQuery] bool? isUpvote,
        Guid suggestionId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetSuggestionVotersQuery(suggestionId, isUpvote)
            {
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize
            },
            cancellationToken);
        return Ok(result);
    }
}

public sealed record SetSuggestionVoteRequest(bool IsUpvote);
