using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Provinces.Queries.GetProvinces;

public sealed class GetProvincesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProvincesQuery, Result<IReadOnlyList<GetProvincesQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetProvincesQueryResponse>>> Handle(GetProvincesQuery request, CancellationToken cancellationToken)
    {
        var provinces = await context.Provinces
            .AsNoTracking()
            .OrderBy(p => p.Name == "İSTANBUL" ? 0 :
                          p.Name == "ANKARA"   ? 1 :
                          p.Name == "İZMİR"    ? 2 :
                          p.Name == "BURSA"    ? 3 : 4)
            .ThenBy(p => p.Name)
            .Select(p => new GetProvincesQueryResponse(p.Id, p.Name))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetProvincesQueryResponse>>.Success(provinces);
    }
}
