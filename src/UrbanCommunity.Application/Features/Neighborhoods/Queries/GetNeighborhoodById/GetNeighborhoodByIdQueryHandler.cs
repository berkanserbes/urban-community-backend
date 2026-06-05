using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodById;

public sealed class GetNeighborhoodByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetNeighborhoodByIdQuery, Result<GetNeighborhoodByIdQueryResponse>>
{
    public async Task<Result<GetNeighborhoodByIdQueryResponse>> Handle(
        GetNeighborhoodByIdQuery request,
        CancellationToken cancellationToken)
    {
        var neighborhood = await context.Neighborhoods
            .AsNoTracking()
            .Where(n => n.Id == request.NeighborhoodId)
            .Select(n => new GetNeighborhoodByIdQueryResponse(
                n.Id,
                n.Name,
                n.DistrictId,
                n.DistrictNavigation != null ? n.DistrictNavigation.Name : null,
                n.DistrictNavigation != null ? (Guid?)n.DistrictNavigation.ProvinceId : null,
                n.DistrictNavigation != null ? n.DistrictNavigation.Province.Name : null))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Neighborhood), request.NeighborhoodId);

        return Result<GetNeighborhoodByIdQueryResponse>.Success(neighborhood);
    }
}
