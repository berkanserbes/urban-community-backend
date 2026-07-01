using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Organizations.Queries.GetOrganizationById;

public sealed class GetOrganizationByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetOrganizationByIdQuery, Result<GetOrganizationByIdQueryResponse>>
{
    public async Task<Result<GetOrganizationByIdQueryResponse>> Handle(
        GetOrganizationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var response = await context.Organizations
            .AsNoTracking()
            .Where(value => value.Id == request.Id)
            .Select(value => new GetOrganizationByIdQueryResponse(
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
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), request.Id);

        return Result<GetOrganizationByIdQueryResponse>.Success(response);
    }
}
