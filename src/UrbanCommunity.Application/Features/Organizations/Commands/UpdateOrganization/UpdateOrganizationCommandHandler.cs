using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Organizations.Commands.UpdateOrganization;

public sealed class UpdateOrganizationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateOrganizationCommand, Result<UpdateOrganizationCommandResponse>>
{
    public async Task<Result<UpdateOrganizationCommandResponse>> Handle(
        UpdateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var organization = await context.Organizations
            .FirstOrDefaultAsync(value => value.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), request.Id);

        var provinceExists = await context.Provinces
            .AsNoTracking()
            .AnyAsync(province => province.Id == request.ProvinceId, cancellationToken);
        if (!provinceExists)
            throw new NotFoundException(nameof(Province), request.ProvinceId);

        if (request.JurisdictionLevel != OrganizationJurisdictionLevel.Province)
        {
            var districtExists = await context.Districts
                .AsNoTracking()
                .AnyAsync(
                    district =>
                        district.Id == request.DistrictId &&
                        district.ProvinceId == request.ProvinceId,
                    cancellationToken);
            if (!districtExists)
                throw new NotFoundException(nameof(District), request.DistrictId!.Value);
        }

        if (request.JurisdictionLevel == OrganizationJurisdictionLevel.Neighborhood)
        {
            var neighborhoodExists = await context.Neighborhoods
                .AsNoTracking()
                .AnyAsync(
                    neighborhood =>
                        neighborhood.Id == request.NeighborhoodId &&
                        neighborhood.DistrictId == request.DistrictId &&
                        neighborhood.DistrictNavigation.ProvinceId == request.ProvinceId,
                    cancellationToken);
            if (!neighborhoodExists)
                throw new NotFoundException(nameof(Neighborhood), request.NeighborhoodId!.Value);
        }

        var normalizedName = request.Name.Trim();
        var nameConflict = await context.Organizations
            .AsNoTracking()
            .AnyAsync(
                value =>
                    value.Id != request.Id &&
                    value.Name == normalizedName &&
                    value.JurisdictionLevel == request.JurisdictionLevel &&
                    value.ProvinceId == request.ProvinceId &&
                    value.DistrictId == request.DistrictId &&
                    value.NeighborhoodId == request.NeighborhoodId,
                cancellationToken);
        if (nameConflict)
            throw new AlreadyExistsException("Aynı konumda bu isimde bir kurum zaten mevcut.");

        organization.Update(
            normalizedName,
            request.JurisdictionLevel,
            request.ProvinceId,
            request.DistrictId,
            request.NeighborhoodId);
        await context.SaveChangesAsync(cancellationToken);

        var response = await context.Organizations
            .AsNoTracking()
            .Where(value => value.Id == organization.Id)
            .Select(value => new UpdateOrganizationCommandResponse(
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
            .SingleAsync(cancellationToken);

        return Result<UpdateOrganizationCommandResponse>.Success(response);
    }
}
