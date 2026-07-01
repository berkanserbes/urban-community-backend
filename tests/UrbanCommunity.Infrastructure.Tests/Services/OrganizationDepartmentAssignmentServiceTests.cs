using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Persistence;
using UrbanCommunity.Infrastructure.Services;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Services;

public sealed class OrganizationDepartmentAssignmentServiceTests
{
    [Fact]
    public async Task ResolveAsync_PrefersNeighborhoodOrganization()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(
            context,
            includeDistrict: true,
            districtCount: 1,
            includeNeighborhood: true);
        var service = new OrganizationDepartmentAssignmentService(context);

        var assignment = await service.ResolveAsync(
            data.Category.Id,
            data.Neighborhood.Id,
            default);

        Assert.Equal(data.NeighborhoodDepartmentId, assignment?.OrganizationDepartmentId);
    }

    [Fact]
    public async Task ResolveAsync_PrefersDistrictOrganization()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context, includeDistrict: true, districtCount: 1);
        var service = new OrganizationDepartmentAssignmentService(context);

        var assignment = await service.ResolveAsync(
            data.Category.Id,
            data.Neighborhood.Id,
            default);

        Assert.Equal(data.DistrictDepartmentId, assignment?.OrganizationDepartmentId);
    }

    [Fact]
    public async Task ResolveAsync_FallsBackToProvinceOrganization()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context, includeDistrict: false, districtCount: 0);
        var service = new OrganizationDepartmentAssignmentService(context);

        var assignment = await service.ResolveAsync(
            data.Category.Id,
            data.Neighborhood.Id,
            default);

        Assert.Equal(data.ProvinceDepartmentId, assignment?.OrganizationDepartmentId);
    }

    [Fact]
    public async Task ResolveAsync_ReturnsNullForAmbiguousPreferredCandidates()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context, includeDistrict: true, districtCount: 2);
        var service = new OrganizationDepartmentAssignmentService(context);

        var assignment = await service.ResolveAsync(
            data.Category.Id,
            data.Neighborhood.Id,
            default);

        Assert.Null(assignment);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<TestData> SeedAsync(
        ApplicationDbContext context,
        bool includeDistrict,
        int districtCount,
        bool includeNeighborhood = false)
    {
        var province = new Province { Name = "Province" };
        var district = new District { Name = "District", Province = province };
        var neighborhood = new Neighborhood
        {
            Name = "Neighborhood",
            District = district.Name,
            City = province.Name,
            DistrictNavigation = district
        };
        var category = new ReportCategory { Name = "Road" };
        var department = new Department { Name = "Public Works" };
        var provinceOrganization = new Organization
        {
            Name = "Province Municipality",
            Province = province,
            ProvinceId = province.Id,
            JurisdictionLevel = OrganizationJurisdictionLevel.Province
        };
        var provinceDepartment = new OrganizationDepartment
        {
            Organization = provinceOrganization,
            Department = department
        };
        var links = new List<OrganizationDepartmentCategory>
        {
            new()
            {
                OrganizationDepartment = provinceDepartment,
                ReportCategory = category
            }
        };
        Guid? firstDistrictDepartmentId = null;
        Guid? neighborhoodDepartmentId = null;

        if (includeDistrict)
        {
            for (var index = 0; index < districtCount; index++)
            {
                var organization = new Organization
                {
                    Name = $"District Municipality {index}",
                    Province = province,
                    ProvinceId = province.Id,
                    District = district,
                    DistrictId = district.Id,
                    JurisdictionLevel = OrganizationJurisdictionLevel.District
                };
                var localDepartment = new OrganizationDepartment
                {
                    Organization = organization,
                    Department = department
                };
                firstDistrictDepartmentId ??= localDepartment.Id;
                links.Add(new OrganizationDepartmentCategory
                {
                    OrganizationDepartment = localDepartment,
                    ReportCategory = category
                });
            }
        }

        if (includeNeighborhood)
        {
            var organization = new Organization
            {
                Name = "Neighborhood Headman's Office",
                Province = province,
                ProvinceId = province.Id,
                District = district,
                DistrictId = district.Id,
                Neighborhood = neighborhood,
                NeighborhoodId = neighborhood.Id,
                JurisdictionLevel = OrganizationJurisdictionLevel.Neighborhood
            };
            var localDepartment = new OrganizationDepartment
            {
                Organization = organization,
                Department = department
            };
            neighborhoodDepartmentId = localDepartment.Id;
            links.Add(new OrganizationDepartmentCategory
            {
                OrganizationDepartment = localDepartment,
                ReportCategory = category
            });
        }

        context.AddRange(
            province,
            neighborhood,
            category,
            department,
            provinceOrganization,
            provinceDepartment);
        context.AddRange(links);
        await context.SaveChangesAsync();

        return new TestData(
            neighborhood,
            category,
            provinceDepartment.Id,
            firstDistrictDepartmentId,
            neighborhoodDepartmentId);
    }

    private sealed record TestData(
        Neighborhood Neighborhood,
        ReportCategory Category,
        Guid ProvinceDepartmentId,
        Guid? DistrictDepartmentId,
        Guid? NeighborhoodDepartmentId);
}
