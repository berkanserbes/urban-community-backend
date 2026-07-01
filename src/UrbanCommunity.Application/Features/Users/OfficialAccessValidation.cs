using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users;

internal sealed record ValidatedOfficialAccess(
    Guid OrganizationDepartmentId,
    string OrganizationName,
    string DepartmentName,
    OfficialJurisdictionLevel JurisdictionLevel,
    Guid JurisdictionId,
    string JurisdictionName,
    Guid? ProvinceId = null,
    Guid? DistrictId = null);

internal static class OfficialAccessValidation
{
    public static async Task<ValidatedOfficialAccess> ValidateAsync(
        IApplicationDbContext context,
        Guid organizationDepartmentId,
        OfficialJurisdictionLevel jurisdictionLevel,
        Guid jurisdictionId,
        CancellationToken cancellationToken)
    {
        var localDepartment = await context.OrganizationDepartments.AsNoTracking()
            .Where(item =>
                item.Id == organizationDepartmentId &&
                item.IsActive &&
                item.Organization.IsActive)
            .Select(item => new
            {
                item.Id,
                item.OrganizationId,
                OrganizationName = item.Organization.Name,
                DepartmentName = item.DisplayName ?? item.Department.Name,
                item.Organization.JurisdictionLevel,
                item.Organization.ProvinceId,
                item.Organization.DistrictId,
                item.Organization.NeighborhoodId
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationDepartment), organizationDepartmentId);

        string? jurisdictionName;
        Guid? provinceId = null;
        Guid? districtId = null;

        switch (jurisdictionLevel)
        {
            case OfficialJurisdictionLevel.Province:
                if (localDepartment.JurisdictionLevel != OrganizationJurisdictionLevel.Province ||
                    jurisdictionId != localDepartment.ProvinceId)
                {
                    throw new ForbiddenAccessException("Official il kapsamı kurum kapsamıyla uyumlu değil.");
                }

                jurisdictionName = await context.Provinces.AsNoTracking()
                    .Where(province => province.Id == jurisdictionId)
                    .Select(province => province.Name)
                    .SingleOrDefaultAsync(cancellationToken);

                provinceId = jurisdictionId;
                break;

            case OfficialJurisdictionLevel.District:
                var district = await context.Districts.AsNoTracking()
                    .Where(item => item.Id == jurisdictionId)
                    .Select(item => new { item.Name, item.ProvinceId })
                    .SingleOrDefaultAsync(cancellationToken);

                if (district is null ||
                    district.ProvinceId != localDepartment.ProvinceId ||
                    localDepartment.JurisdictionLevel == OrganizationJurisdictionLevel.Neighborhood ||
                    (localDepartment.JurisdictionLevel == OrganizationJurisdictionLevel.District &&
                     jurisdictionId != localDepartment.DistrictId))
                {
                    throw new ForbiddenAccessException("Official ilçe kapsamı kurum kapsamıyla uyumlu değil.");
                }

                jurisdictionName = district.Name;
                provinceId = district.ProvinceId;
                districtId = jurisdictionId;
                break;

            case OfficialJurisdictionLevel.Neighborhood:
                var neighborhood = await context.Neighborhoods.AsNoTracking()
                    .Where(item => item.Id == jurisdictionId)
                    .Select(item => new
                    {
                        item.Name,
                        item.DistrictId,
                        item.DistrictNavigation.ProvinceId
                    })
                    .SingleOrDefaultAsync(cancellationToken);

                if (neighborhood is null ||
                    neighborhood.ProvinceId != localDepartment.ProvinceId ||
                    (localDepartment.JurisdictionLevel == OrganizationJurisdictionLevel.District &&
                     neighborhood.DistrictId != localDepartment.DistrictId) ||
                    (localDepartment.JurisdictionLevel == OrganizationJurisdictionLevel.Neighborhood &&
                     jurisdictionId != localDepartment.NeighborhoodId))
                {
                    throw new ForbiddenAccessException("Official mahalle kapsamı kurum kapsamıyla uyumlu değil.");
                }

                jurisdictionName = neighborhood.Name;
                provinceId = neighborhood.ProvinceId;
                districtId = neighborhood.DistrictId;
                break;

            default:
                throw new ForbiddenAccessException("Geçersiz Official yetki alanı.");
        }

        return new ValidatedOfficialAccess(
            localDepartment.Id,
            localDepartment.OrganizationName,
            localDepartment.DepartmentName,
            jurisdictionLevel,
            jurisdictionId,
            jurisdictionName ?? throw new NotFoundException(jurisdictionLevel.ToString(), jurisdictionId),
            provinceId,
            districtId);
    }
}
