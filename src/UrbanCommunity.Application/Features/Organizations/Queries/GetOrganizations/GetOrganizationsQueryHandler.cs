using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Organizations.Queries.GetOrganizations;

public sealed class GetOrganizationsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<
        GetOrganizationsQuery,
        Result<IReadOnlyList<GetOrganizationsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetOrganizationsQueryResponse>>> Handle(
        GetOrganizationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Organizations.AsNoTracking();
        if (request.IsActive.HasValue)
            query = query.Where(value => value.IsActive == request.IsActive.Value);

        var organizations = await query
            .OrderBy(value => value.Name)
            .Select(value => new GetOrganizationsQueryResponse(
                value.Id,
                value.Name,
                value.JurisdictionLevel,
                value.ProvinceId,
                value.Province.Name,
                value.DistrictId,
                value.District != null ? value.District.Name : null,
                value.NeighborhoodId,
                value.Neighborhood != null ? value.Neighborhood.Name : null,
                value.IsActive,
                value.Departments.Count,
                value.Departments.SelectMany(department => department.Officials).Count()))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetOrganizationsQueryResponse>>.Success(organizations);
    }
}
