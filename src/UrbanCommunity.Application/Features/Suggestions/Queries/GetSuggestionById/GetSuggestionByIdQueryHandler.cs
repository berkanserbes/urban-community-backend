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
            .Select(s => new GetSuggestionByIdQueryResponse(
                s.Id,
                s.Title,
                s.Content,
                s.Status,
                s.VoteScore,
                s.UserId,
                (s.User.FirstName + " " + s.User.LastName).Trim(),
                s.Votes.Count(v => v.IsUpvote),
                s.Votes.Count(v => !v.IsUpvote),
                s.CreatedAt,
                s.UpdatedAt))
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Suggestion), request.Id);

        return Result<GetSuggestionByIdQueryResponse>.Success(suggestion);
    }
}
