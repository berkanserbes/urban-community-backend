using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoods;

public sealed class GetNeighborhoodsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetNeighborhoodsQuery, Result<IReadOnlyList<GetNeighborhoodsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetNeighborhoodsQueryResponse>>> Handle(GetNeighborhoodsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Neighborhoods.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(n => n.City == request.City);

        if (!string.IsNullOrWhiteSpace(request.District))
            query = query.Where(n => n.District == request.District);

        var neighborhoods = await query
            .OrderBy(n => n.City)
            .ThenBy(n => n.District)
            .ThenBy(n => n.Name)
            .Select(n => new GetNeighborhoodsQueryResponse(n.Id, n.Name, n.District, n.City))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetNeighborhoodsQueryResponse>>.Success(neighborhoods);
    }
}
