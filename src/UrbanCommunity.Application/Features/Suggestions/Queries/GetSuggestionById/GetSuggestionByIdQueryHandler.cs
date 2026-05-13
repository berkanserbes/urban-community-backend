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
            .Include(s => s.Votes)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Suggestion), request.Id);

        var upVotes = suggestion.Votes.Count(v => v.IsUpvote);
        var downVotes = suggestion.Votes.Count(v => !v.IsUpvote);

        return Result<GetSuggestionByIdQueryResponse>.Success(
            new GetSuggestionByIdQueryResponse(
                suggestion.Id,
                suggestion.Title,
                suggestion.Content,
                suggestion.Status,
                suggestion.VoteScore,
                suggestion.UserId,
                suggestion.User.FullName,
                upVotes,
                downVotes,
                suggestion.CreatedAt,
                suggestion.UpdatedAt));
    }
}
