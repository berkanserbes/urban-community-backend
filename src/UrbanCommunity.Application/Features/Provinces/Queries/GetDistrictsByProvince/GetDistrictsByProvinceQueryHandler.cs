using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Provinces.Queries.GetDistrictsByProvince;

public sealed class GetDistrictsByProvinceQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDistrictsByProvinceQuery, Result<IReadOnlyList<GetDistrictsByProvinceQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetDistrictsByProvinceQueryResponse>>> Handle(GetDistrictsByProvinceQuery request, CancellationToken cancellationToken)
    {
        var provinceExists = await context.Provinces
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.ProvinceId, cancellationToken);

        if (!provinceExists)
            throw new NotFoundException(nameof(Province), request.ProvinceId);

        var districts = await context.Districts
            .AsNoTracking()
            .Where(d => d.ProvinceId == request.ProvinceId)
            .OrderBy(d => d.Name)
            .Select(d => new GetDistrictsByProvinceQueryResponse(d.Id, d.Name, d.ProvinceId))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetDistrictsByProvinceQueryResponse>>.Success(districts);
    }
}
