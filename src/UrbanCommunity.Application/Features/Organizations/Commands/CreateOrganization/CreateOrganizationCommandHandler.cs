using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Organizations.Commands.CreateOrganization;

public sealed class CreateOrganizationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateOrganizationCommand, Result<CreateOrganizationCommandResponse>>
{
    public async Task<Result<CreateOrganizationCommandResponse>> Handle(
        CreateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
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
        var organizationExists = await context.Organizations
            .AsNoTracking()
            .AnyAsync(
                organization =>
                    organization.Name == normalizedName &&
                    organization.JurisdictionLevel == request.JurisdictionLevel &&
                    organization.ProvinceId == request.ProvinceId &&
                    organization.DistrictId == request.DistrictId &&
                    organization.NeighborhoodId == request.NeighborhoodId,
                cancellationToken);
        if (organizationExists)
            throw new AlreadyExistsException("Aynı konumda bu isimde bir kurum zaten mevcut.");

        var organization = new Organization();
        organization.Update(
            normalizedName,
            request.JurisdictionLevel,
            request.ProvinceId,
            request.DistrictId,
            request.NeighborhoodId);

        context.Organizations.Add(organization);
        await context.SaveChangesAsync(cancellationToken);

        var response = await context.Organizations
            .AsNoTracking()
            .Where(value => value.Id == organization.Id)
            .Select(value => new CreateOrganizationCommandResponse(
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

        return Result<CreateOrganizationCommandResponse>.Success(response);
    }
}
