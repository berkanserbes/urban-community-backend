using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestionStats;

public sealed class GetSuggestionStatsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSuggestionStatsQuery, Result<GetSuggestionStatsQueryResponse>>
{
    public async Task<Result<GetSuggestionStatsQueryResponse>> Handle(
        GetSuggestionStatsQuery request,
        CancellationToken cancellationToken)
    {
        var stats = await context.Suggestions
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(suggestions => new GetSuggestionStatsQueryResponse(
                suggestions.Count(),
                suggestions.Count(s => s.Status == SuggestionStatus.Active),
                suggestions.Count(s => s.Status == SuggestionStatus.Reviewing),
                suggestions.Count(s => s.Status == SuggestionStatus.Implementation),
                suggestions.Count(s => s.Status == SuggestionStatus.Closed)))
            .FirstOrDefaultAsync(cancellationToken);

        return Result<GetSuggestionStatsQueryResponse>.Success(stats ?? GetSuggestionStatsQueryResponse.Empty);
    }
}
