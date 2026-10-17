using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUserStats;

public sealed class GetUserStatsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetUserStatsQuery, Result<GetUserStatsQueryResponse>>
{
    public async Task<Result<GetUserStatsQueryResponse>> Handle(
        GetUserStatsQuery request,
        CancellationToken cancellationToken)
    {
        var stats = await context.Users
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(users => new GetUserStatsQueryResponse(
                users.Count(),
                users.Count(user => user.IsActive),
                users.Count(user => !user.IsActive),
                users.Count(user => user.Role == UserRole.Admin),
                users.Count(user => user.Role == UserRole.Official),
                users.Count(user => user.Role == UserRole.Citizen)))
            .FirstOrDefaultAsync(cancellationToken);

        return Result<GetUserStatsQueryResponse>.Success(stats ?? GetUserStatsQueryResponse.Empty);
    }
}
