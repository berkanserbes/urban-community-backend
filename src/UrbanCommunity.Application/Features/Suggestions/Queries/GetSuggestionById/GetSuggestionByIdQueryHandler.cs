using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestionById;

public sealed class GetSuggestionByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSuggestionByIdQuery, Result<GetSuggestionByIdQueryResponse>>
{
    public async Task<Result<GetSuggestionByIdQueryResponse>> Handle(GetSuggestionByIdQuery request, CancellationToken cancellationToken)
    {
        var suggestion = await context.Suggestions
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Suggestion), request.Id);

        var upvoteCount = await context.Votes
            .CountAsync(v => v.SuggestionId == suggestion.Id && v.IsUpvote, cancellationToken);

        var downvoteCount = await context.Votes
            .CountAsync(v => v.SuggestionId == suggestion.Id && !v.IsUpvote, cancellationToken);

        var response = new GetSuggestionByIdQueryResponse(
            suggestion.Id,
            suggestion.Title,
            suggestion.Content,
            suggestion.OfficialResponse,
            suggestion.Status,
            suggestion.VoteScore,
            suggestion.UserId,
            (suggestion.User.FirstName + " " + suggestion.User.LastName).Trim(),
            upvoteCount,
            downvoteCount,
            suggestion.CreatedAt,
            suggestion.UpdatedAt);

        return Result<GetSuggestionByIdQueryResponse>.Success(response);
    }
}
