using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Services;

public sealed class OrganizationDepartmentAssignmentService(
    IApplicationDbContext context,
    ILogger<OrganizationDepartmentAssignmentService>? logger = null)
    : IOrganizationDepartmentAssignmentService
{
    public async Task<OrganizationDepartmentAssignment?> ResolveAsync(
        Guid categoryId,
        Guid neighborhoodId,
        CancellationToken cancellationToken)
    {
        var location = await context.Neighborhoods.AsNoTracking()
            .Where(neighborhood => neighborhood.Id == neighborhoodId)
            .Select(neighborhood => new
            {
                neighborhood.Id,
                neighborhood.DistrictId,
                neighborhood.DistrictNavigation.ProvinceId
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (location is null)
            return null;

        var candidates = await BaseCandidates(categoryId)
            .Where(item =>
                (item.Organization.JurisdictionLevel == OrganizationJurisdictionLevel.Neighborhood &&
                 item.Organization.NeighborhoodId == location.Id) ||
                (item.Organization.JurisdictionLevel == OrganizationJurisdictionLevel.District &&
                 item.Organization.DistrictId == location.DistrictId) ||
                (item.Organization.JurisdictionLevel == OrganizationJurisdictionLevel.Province &&
                 item.Organization.ProvinceId == location.ProvinceId))
            .Select(item => new
            {
                Assignment = new OrganizationDepartmentAssignment(
                    item.Id,
                    item.OrganizationId,
                    item.Organization.Name,
                    item.DepartmentId,
                    item.DisplayName ?? item.Department.Name,
                    item.ContactEmail,
                    item.ContactPhone),
                item.Organization.JurisdictionLevel
            })
            .ToListAsync(cancellationToken);

        var preferredLevel = candidates.Count > 0
            ? candidates.Max(candidate => candidate.JurisdictionLevel)
            : default;
        var preferred = candidates
            .Where(candidate => candidate.JurisdictionLevel == preferredLevel)
            .ToList();

        if (preferred.Count > 1)
        {
            logger?.LogWarning(
                "Automatic report assignment is ambiguous for category {CategoryId}, " +
                "neighborhood {NeighborhoodId}. Candidate organization departments: {CandidateIds}",
                categoryId,
                neighborhoodId,
                preferred.Select(candidate => candidate.Assignment.OrganizationDepartmentId));
        }

        return preferred.Count == 1 ? preferred[0].Assignment : null;
    }

    public async Task<OrganizationDepartmentAssignment?> GetCompatibleAsync(
        Guid organizationDepartmentId,
        Guid neighborhoodId,
        CancellationToken cancellationToken)
    {
        var location = await context.Neighborhoods.AsNoTracking()
            .Where(neighborhood => neighborhood.Id == neighborhoodId)
            .Select(neighborhood => new
            {
                neighborhood.Id,
                neighborhood.DistrictId,
                neighborhood.DistrictNavigation.ProvinceId
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (location is null)
            return null;

        return await context.OrganizationDepartments.AsNoTracking()
            .Where(value =>
                value.Id == organizationDepartmentId &&
                value.IsActive &&
                value.Organization.IsActive &&
                ((value.Organization.JurisdictionLevel == OrganizationJurisdictionLevel.Neighborhood &&
                  value.Organization.NeighborhoodId == location.Id) ||
                 (value.Organization.JurisdictionLevel == OrganizationJurisdictionLevel.District &&
                  value.Organization.DistrictId == location.DistrictId) ||
                 (value.Organization.JurisdictionLevel == OrganizationJurisdictionLevel.Province &&
                  value.Organization.ProvinceId == location.ProvinceId)))
            .Select(value => new OrganizationDepartmentAssignment(
                value.Id,
                value.OrganizationId,
                value.Organization.Name,
                value.DepartmentId,
                value.DisplayName ?? value.Department.Name,
                value.ContactEmail,
                value.ContactPhone))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsCompatibleAsync(
        Guid organizationDepartmentId,
        Guid categoryId,
        Guid neighborhoodId,
        CancellationToken cancellationToken)
    {
        if (await GetCompatibleAsync(
                organizationDepartmentId,
                neighborhoodId,
                cancellationToken) is null)
        {
            return false;
        }

        return await context.OrganizationDepartmentCategories.AsNoTracking()
            .AnyAsync(
                link =>
                    link.OrganizationDepartmentId == organizationDepartmentId &&
                    link.ReportCategoryId == categoryId,
                cancellationToken);
    }

    private IQueryable<Domain.Entities.OrganizationDepartment> BaseCandidates(Guid categoryId)
    {
        return context.OrganizationDepartments.AsNoTracking()
            .Where(item =>
                item.IsActive &&
                item.Organization.IsActive &&
                item.Categories.Any(link => link.ReportCategoryId == categoryId));
    }
}
