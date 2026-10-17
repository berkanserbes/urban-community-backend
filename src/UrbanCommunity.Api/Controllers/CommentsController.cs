using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.Comments.Commands.CommunityPostComment.AddCommunityPostComment;
using UrbanCommunity.Application.Features.Comments.Commands.CommunityPostComment.DeleteCommunityPostComment;
using UrbanCommunity.Application.Features.Comments.Commands.CommunityPostComment.UpdateCommunityPostComment;
using UrbanCommunity.Application.Features.Comments.Commands.ReportComment.AddReportComment;
using UrbanCommunity.Application.Features.Comments.Commands.ReportComment.DeleteReportComment;
using UrbanCommunity.Application.Features.Comments.Commands.ReportComment.UpdateReportComment;
using UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.AddSuggestionComment;
using UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.DeleteSuggestionComment;
using UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.UpdateSuggestionComment;
using UrbanCommunity.Application.Features.Comments.Queries.CommunityPostComment.GetCommunityPostCommentById;
using UrbanCommunity.Application.Features.Comments.Queries.CommunityPostComment.GetCommunityPostComments;
using UrbanCommunity.Application.Features.Comments.Queries.ReportComment.GetReportCommentById;
using UrbanCommunity.Application.Features.Comments.Queries.ReportComment.GetReportComments;
using UrbanCommunity.Application.Features.Comments.Queries.SuggestionComment.GetSuggestionCommentById;
using UrbanCommunity.Application.Features.Comments.Queries.SuggestionComment.GetSuggestionComments;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class CommentsController(IMediator mediator) : ApiController
{
    [HttpGet("reports/{reportId:guid}/comments")]
    [ProducesResponseType(typeof(Result<PagedResult<GetReportCommentsQueryResponse>>), 200)]
    public async Task<IActionResult> GetReportComments(
        [FromQuery] PaginationRequest pagination,
        Guid reportId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetReportCommentsQuery(reportId)
            {
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize
            },
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("report-comments/{commentId:guid}")]
    public async Task<IActionResult> GetReportCommentById(Guid commentId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetReportCommentByIdQuery(commentId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("reports/{reportId:guid}/comments")]
    public async Task<IActionResult> AddReportComment(
        Guid reportId,
        [FromBody] CommentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new AddReportCommentCommand(reportId, userId, request.Content), cancellationToken);
        return CreatedAtAction(nameof(GetReportCommentById), new { commentId = result.Data?.Id }, result);
    }

    [HttpPut("report-comments/{commentId:guid}")]
    public async Task<IActionResult> UpdateReportComment(
        Guid commentId,
        [FromBody] CommentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new UpdateReportCommentCommand(commentId, userId, request.Content), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("report-comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteReportComment(Guid commentId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await mediator.Send(new DeleteReportCommentCommand(commentId, userId, isAdmin), cancellationToken);
        return Ok(result);
    }

    [HttpGet("suggestions/{suggestionId:guid}/comments")]
    [ProducesResponseType(typeof(Result<PagedResult<GetSuggestionCommentsQueryResponse>>), 200)]
    public async Task<IActionResult> GetSuggestionComments(
        [FromQuery] PaginationRequest pagination,
        Guid suggestionId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetSuggestionCommentsQuery(suggestionId)
            {
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize
            },
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("suggestion-comments/{commentId:guid}")]
    public async Task<IActionResult> GetSuggestionCommentById(Guid commentId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSuggestionCommentByIdQuery(commentId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("suggestions/{suggestionId:guid}/comments")]
    public async Task<IActionResult> AddSuggestionComment(
        Guid suggestionId,
        [FromBody] CommentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new AddSuggestionCommentCommand(suggestionId, userId, request.Content), cancellationToken);
        return CreatedAtAction(nameof(GetSuggestionCommentById), new { commentId = result.Data?.Id }, result);
    }

    [HttpPut("suggestion-comments/{commentId:guid}")]
    public async Task<IActionResult> UpdateSuggestionComment(
        Guid commentId,
        [FromBody] CommentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new UpdateSuggestionCommentCommand(commentId, userId, request.Content), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("suggestion-comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteSuggestionComment(Guid commentId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await mediator.Send(new DeleteSuggestionCommentCommand(commentId, userId, isAdmin), cancellationToken);
        return Ok(result);
    }

    [HttpGet("community-posts/{communityPostId:guid}/comments")]
    [ProducesResponseType(typeof(Result<PagedResult<GetCommunityPostCommentsQueryResponse>>), 200)]
    public async Task<IActionResult> GetCommunityPostComments(
        [FromQuery] PaginationRequest pagination,
        Guid communityPostId,
        CancellationToken cancellationToken)
    {
        var requestingUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await mediator.Send(
            new GetCommunityPostCommentsQuery(communityPostId, requestingUserId, isAdmin)
            {
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize
            },
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("community-post-comments/{commentId:guid}")]
    public async Task<IActionResult> GetCommunityPostCommentById(Guid commentId, CancellationToken cancellationToken)
    {
        var requestingUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await mediator.Send(new GetCommunityPostCommentByIdQuery(commentId, requestingUserId, isAdmin), cancellationToken);
        return Ok(result);
    }

    [HttpPost("community-posts/{communityPostId:guid}/comments")]
    public async Task<IActionResult> AddCommunityPostComment(
        Guid communityPostId,
        [FromBody] CommentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new AddCommunityPostCommentCommand(communityPostId, userId, request.Content), cancellationToken);
        return CreatedAtAction(nameof(GetCommunityPostCommentById), new { commentId = result.Data?.Id }, result);
    }

    [HttpPut("community-post-comments/{commentId:guid}")]
    public async Task<IActionResult> UpdateCommunityPostComment(
        Guid commentId,
        [FromBody] CommentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new UpdateCommunityPostCommentCommand(commentId, userId, request.Content), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("community-post-comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteCommunityPostComment(Guid commentId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await mediator.Send(new DeleteCommunityPostCommentCommand(commentId, userId, isAdmin), cancellationToken);
        return Ok(result);
    }
}

public sealed record CommentRequest(string Content);
