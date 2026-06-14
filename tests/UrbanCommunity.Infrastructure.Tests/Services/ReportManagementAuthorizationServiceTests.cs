using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Persistence;
using UrbanCommunity.Infrastructure.Services;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Services;

public sealed class ReportManagementAuthorizationServiceTests
{
    [Theory]
    [InlineData(OfficialJurisdictionLevel.Province, 3)]
    [InlineData(OfficialJurisdictionLevel.District, 2)]
    [InlineData(OfficialJurisdictionLevel.Neighborhood, 1)]
    public async Task ApplyManageableScope_FiltersByJurisdictionHierarchy(
        OfficialJurisdictionLevel level,
        int expectedCount)
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context);
        var official = CreateOfficial(data, level, data.PrimaryDepartment.Id);
        context.Users.Add(official);
        await context.SaveChangesAsync();
        var service = new ReportManagementAuthorizationService(context);

        var access = await service.GetAccessAsync(official.Id, false, default);
        var reports = await service
            .ApplyManageableScope(context.Reports.AsNoTracking(), access)
            .ToListAsync();

        Assert.Equal(expectedCount, reports.Count);
    }

    [Fact]
    public async Task ApplyManageableScope_ExcludesReportsAssignedToAnotherDepartment()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context);
        var official = CreateOfficial(
            data,
            OfficialJurisdictionLevel.Province,
            data.PrimaryDepartment.Id);
        context.Users.Add(official);

        data.SameNeighborhoodReport.AssignedDepartmentId = data.OtherDepartment.Id;
        data.SameDistrictReport.AssignedDepartmentId = data.PrimaryDepartment.Id;
        await context.SaveChangesAsync();
        var service = new ReportManagementAuthorizationService(context);

        var access = await service.GetAccessAsync(official.Id, false, default);
        var reportIds = await service
            .ApplyManageableScope(context.Reports.AsNoTracking(), access)
            .Select(report => report.Id)
            .ToListAsync();

        Assert.DoesNotContain(data.SameNeighborhoodReport.Id, reportIds);
        Assert.Contains(data.SameDistrictReport.Id, reportIds);
        Assert.Contains(data.SameProvinceReport.Id, reportIds);
    }

    [Fact]
    public async Task ApplyManageableScope_AdminCanManageAllReports()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var admin = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Global",
            LastName = "Admin",
            UserName = "admin@example.com",
            Email = "admin@example.com"
        };
        admin.UpdateAccess(UserRole.Admin);
        context.Users.Add(admin);
        await context.SaveChangesAsync();
        var service = new ReportManagementAuthorizationService(context);

        var access = await service.GetAccessAsync(admin.Id, true, default);
        var reportCount = await service
            .ApplyManageableScope(context.Reports.AsNoTracking(), access)
            .CountAsync();

        Assert.Equal(4, reportCount);
    }

    [Fact]
    public async Task EnsureCanManageAsync_ThrowsForbiddenOutsideJurisdiction()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context);
        var official = CreateOfficial(
            data,
            OfficialJurisdictionLevel.Neighborhood,
            data.PrimaryDepartment.Id);
        context.Users.Add(official);
        await context.SaveChangesAsync();
        var service = new ReportManagementAuthorizationService(context);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.EnsureCanManageAsync(
                data.SameDistrictReport.Id,
                official.Id,
                false,
                default));
    }

    [Fact]
    public async Task WhereJurisdictionMatches_ReturnsOfficialsAtEveryMatchingLevel()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context);
        var provinceOfficial = CreateOfficial(
            data,
            OfficialJurisdictionLevel.Province,
            data.PrimaryDepartment.Id);
        var districtOfficial = CreateOfficial(
            data,
            OfficialJurisdictionLevel.District,
            data.PrimaryDepartment.Id);
        var neighborhoodOfficial = CreateOfficial(
            data,
            OfficialJurisdictionLevel.Neighborhood,
            data.PrimaryDepartment.Id);
        var otherProvinceOfficial = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Other",
            LastName = "Official",
            UserName = $"other-{Guid.NewGuid():N}@example.com",
            Email = $"other-{Guid.NewGuid():N}@example.com"
        };
        otherProvinceOfficial.UpdateAccess(
            UserRole.Official,
            data.PrimaryDepartment.Id,
            OfficialJurisdictionLevel.Province,
            data.OtherProvince.Id);

        context.Users.AddRange(
            provinceOfficial,
            districtOfficial,
            neighborhoodOfficial,
            otherProvinceOfficial);
        await context.SaveChangesAsync();

        var matchedIds = await context.Users
            .Where(user => user.Role == UserRole.Official)
            .WhereJurisdictionMatches(
                data.PrimaryProvince.Id,
                data.PrimaryDistrict.Id,
                data.PrimaryNeighborhood.Id)
            .Select(user => user.Id)
            .ToListAsync();

        Assert.Equal(3, matchedIds.Count);
        Assert.Contains(provinceOfficial.Id, matchedIds);
        Assert.Contains(districtOfficial.Id, matchedIds);
        Assert.Contains(neighborhoodOfficial.Id, matchedIds);
        Assert.DoesNotContain(otherProvinceOfficial.Id, matchedIds);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static User CreateOfficial(
        TestData data,
        OfficialJurisdictionLevel level,
        Guid departmentId)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = level.ToString(),
            LastName = "Official",
            UserName = $"{level}-{Guid.NewGuid():N}@example.com",
            Email = $"{level}-{Guid.NewGuid():N}@example.com"
        };

        var jurisdictionId = level switch
        {
            OfficialJurisdictionLevel.Province => data.PrimaryProvince.Id,
            OfficialJurisdictionLevel.District => data.PrimaryDistrict.Id,
            OfficialJurisdictionLevel.Neighborhood => data.PrimaryNeighborhood.Id,
            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };

        user.UpdateAccess(
            UserRole.Official,
            departmentId,
            level,
            jurisdictionId);

        return user;
    }

    private static async Task<TestData> SeedAsync(ApplicationDbContext context)
    {
        var primaryProvince = new Province { Name = "Province A" };
        var otherProvince = new Province { Name = "Province B" };
        var primaryDistrict = new District
        {
            Name = "District A1",
            Province = primaryProvince
        };
        var sameProvinceDistrict = new District
        {
            Name = "District A2",
            Province = primaryProvince
        };
        var otherDistrict = new District
        {
            Name = "District B1",
            Province = otherProvince
        };
        var primaryNeighborhood = CreateNeighborhood(
            "Neighborhood A1",
            primaryDistrict,
            "Province A");
        var sameDistrictNeighborhood = CreateNeighborhood(
            "Neighborhood A2",
            primaryDistrict,
            "Province A");
        var sameProvinceNeighborhood = CreateNeighborhood(
            "Neighborhood A3",
            sameProvinceDistrict,
            "Province A");
        var otherProvinceNeighborhood = CreateNeighborhood(
            "Neighborhood B1",
            otherDistrict,
            "Province B");
        var owner = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Report",
            LastName = "Owner",
            UserName = "owner@example.com",
            Email = "owner@example.com"
        };
        var category = new ReportCategory { Name = "Test" };
        var primaryDepartment = new Department { Name = "Primary Department" };
        var otherDepartment = new Department { Name = "Other Department" };

        var sameNeighborhoodReport = CreateReport(
            "Same neighborhood",
            primaryNeighborhood,
            owner,
            category);
        var sameDistrictReport = CreateReport(
            "Same district",
            sameDistrictNeighborhood,
            owner,
            category);
        var sameProvinceReport = CreateReport(
            "Same province",
            sameProvinceNeighborhood,
            owner,
            category);
        var otherProvinceReport = CreateReport(
            "Other province",
            otherProvinceNeighborhood,
            owner,
            category);

        context.AddRange(
            primaryProvince,
            otherProvince,
            primaryDepartment,
            otherDepartment,
            owner,
            category,
            sameNeighborhoodReport,
            sameDistrictReport,
            sameProvinceReport,
            otherProvinceReport);
        await context.SaveChangesAsync();

        return new TestData(
            primaryProvince,
            otherProvince,
            primaryDistrict,
            primaryNeighborhood,
            primaryDepartment,
            otherDepartment,
            sameNeighborhoodReport,
            sameDistrictReport,
            sameProvinceReport);
    }

    private static Neighborhood CreateNeighborhood(
        string name,
        District district,
        string city)
    {
        return new Neighborhood
        {
            Name = name,
            District = district.Name,
            City = city,
            DistrictNavigation = district
        };
    }

    private static Report CreateReport(
        string title,
        Neighborhood neighborhood,
        User owner,
        ReportCategory category)
    {
        return new Report
        {
            Title = title,
            Description = "Test report description",
            Location = new Point(29, 41) { SRID = 4326 },
            User = owner,
            Category = category,
            Neighborhood = neighborhood
        };
    }

    private sealed record TestData(
        Province PrimaryProvince,
        Province OtherProvince,
        District PrimaryDistrict,
        Neighborhood PrimaryNeighborhood,
        Department PrimaryDepartment,
        Department OtherDepartment,
        Report SameNeighborhoodReport,
        Report SameDistrictReport,
        Report SameProvinceReport);
}
