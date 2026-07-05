using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Persistence;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Persistence;

public sealed class AuditInterceptorTests
{
    [Fact]
    public async Task SaveChanges_WritesOrganizationAuditRecord()
    {
        await using var context = CreateContext();
        var province = new Province { Name = "Test Province" };
        var organization = new Organization
        {
            Name = "Test Municipality",
            Province = province,
            ProvinceId = province.Id,
            JurisdictionLevel = OrganizationJurisdictionLevel.Province
        };

        context.AddRange(province, organization);
        await context.SaveChangesAsync();

        var audit = await context.AuditLogs.AsNoTracking()
            .SingleAsync(value => value.EntityType == nameof(Organization));
        Assert.Equal(EntityState.Added.ToString(), audit.Action);
        Assert.Equal(organization.Id.ToString(), audit.EntityId);
        Assert.Contains("Test Municipality", audit.NewValues);
        Assert.DoesNotContain("Password", audit.NewValues);
    }

    [Fact]
    public async Task SaveChanges_WritesAuditRecordsForManagedDomainEntities()
    {
        await using var context = CreateContext();
        var data = CreateDomainGraph();

        context.AddRange(
            data.Province,
            data.District,
            data.Neighborhood,
            data.User,
            data.Department,
            data.Category,
            data.Organization,
            data.OrganizationDepartment,
            data.OrganizationDepartmentCategory,
            data.Report,
            data.Suggestion,
            data.Announcement,
            data.CommunityPost);

        await context.SaveChangesAsync();

        var auditedEntityTypes = await context.AuditLogs
            .AsNoTracking()
            .Select(value => value.EntityType)
            .ToListAsync();

        string[] expectedAuditedTypes =
        [
            nameof(Announcement),
            nameof(CommunityPost),
            nameof(Department),
            nameof(Organization),
            nameof(OrganizationDepartment),
            nameof(OrganizationDepartmentCategory),
            nameof(Report),
            nameof(ReportCategory),
            nameof(Suggestion),
            nameof(User)
        ];

        foreach (var expectedAuditedType in expectedAuditedTypes)
        {
            Assert.Contains(expectedAuditedType, auditedEntityTypes);
        }

        Assert.DoesNotContain(nameof(Province), auditedEntityTypes);
        Assert.DoesNotContain(nameof(District), auditedEntityTypes);
        Assert.DoesNotContain(nameof(Neighborhood), auditedEntityTypes);
    }

    [Fact]
    public async Task SaveChanges_WritesReportLocationAuditAsJsonSafeValue()
    {
        await using var context = CreateContext();
        var data = CreateDomainGraph();
        data.Report.Location = new Point(29.062753837240315, 40.29688491208338) { SRID = 4326 };

        context.AddRange(
            data.Province,
            data.District,
            data.Neighborhood,
            data.User,
            data.Category,
            data.Report);

        await context.SaveChangesAsync();

        var audit = await context.AuditLogs.AsNoTracking()
            .SingleAsync(value => value.EntityType == nameof(Report));

        Assert.Contains("\"Location\"", audit.NewValues);
        Assert.Contains("\"longitude\":29.062753837240315", audit.NewValues);
        Assert.Contains("\"latitude\":40.29688491208338", audit.NewValues);
        Assert.Contains("\"srid\":4326", audit.NewValues);
    }

    [Fact]
    public async Task SaveChanges_DoesNotAuditOperationalHighVolumeEntities()
    {
        await using var context = CreateContext();
        var data = CreateDomainGraph();

        context.AddRange(
            data.Province,
            data.District,
            data.Neighborhood,
            data.User,
            data.Category,
            data.Report,
            data.Suggestion,
            data.CommunityPost);

        await context.SaveChangesAsync();
        context.AuditLogs.RemoveRange(context.AuditLogs);
        await context.SaveChangesAsync();

        context.AddRange(
            new ReportComment
            {
                ReportId = data.Report.Id,
                UserId = data.User.Id,
                Content = "Report comment"
            },
            new SuggestionComment
            {
                SuggestionId = data.Suggestion.Id,
                UserId = data.User.Id,
                Content = "Suggestion comment"
            },
            new CommunityPostComment
            {
                CommunityPostId = data.CommunityPost.Id,
                UserId = data.User.Id,
                Content = "Community post comment"
            },
            new Vote
            {
                SuggestionId = data.Suggestion.Id,
                UserId = data.User.Id,
                IsUpvote = true
            },
            new Notification
            {
                UserId = data.User.Id,
                Title = "Notification",
                Message = "Notification message"
            },
            new UserRefreshToken
            {
                UserId = data.User.Id,
                Token = "token",
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            });

        await context.SaveChangesAsync();

        Assert.Empty(await context.AuditLogs.AsNoTracking().ToListAsync());
    }

    private static ApplicationDbContext CreateContext()
    {
        var interceptor = new AuditInterceptor(new HttpContextAccessor());
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;
        return new ApplicationDbContext(options);
    }

    private static TestDomainGraph CreateDomainGraph()
    {
        var province = new Province { Name = "Audit Province" };
        var district = new District
        {
            Name = "Audit District",
            ProvinceId = province.Id,
            Province = province
        };
        var neighborhood = new Neighborhood
        {
            Name = "Audit Neighborhood",
            District = district.Name,
            City = province.Name,
            DistrictId = district.Id,
            DistrictNavigation = district
        };
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Audit",
            LastName = "User",
            UserName = $"{Guid.NewGuid():N}@example.com",
            Email = $"{Guid.NewGuid():N}@example.com",
            Role = UserRole.Citizen,
            IsActive = true
        };
        var department = new Department
        {
            Name = "Audit Department"
        };
        var category = new ReportCategory
        {
            Name = "Audit Category"
        };
        var organization = new Organization
        {
            Name = "Audit Municipality",
            ProvinceId = province.Id,
            Province = province,
            JurisdictionLevel = OrganizationJurisdictionLevel.Province
        };
        var organizationDepartment = new OrganizationDepartment
        {
            OrganizationId = organization.Id,
            Organization = organization,
            DepartmentId = department.Id,
            Department = department,
            DisplayName = "Audit Department Display"
        };
        var organizationDepartmentCategory = new OrganizationDepartmentCategory
        {
            OrganizationDepartmentId = organizationDepartment.Id,
            OrganizationDepartment = organizationDepartment,
            ReportCategoryId = category.Id,
            ReportCategory = category
        };
        var report = new Report
        {
            Title = "Audit Report",
            Description = "Audit report description",
            UserId = user.Id,
            User = user,
            CategoryId = category.Id,
            Category = category,
            NeighborhoodId = neighborhood.Id,
            Neighborhood = neighborhood
        };
        var suggestion = new Suggestion
        {
            Title = "Audit Suggestion",
            Content = "Audit suggestion content",
            UserId = user.Id,
            User = user
        };
        var announcement = new Announcement
        {
            Title = "Audit Announcement",
            Content = "Audit announcement content",
            UserId = user.Id,
            User = user
        };
        var communityPost = new CommunityPost
        {
            Type = PostType.HelpRequested,
            Title = "Audit Community Post",
            Content = "Audit community post content",
            UserId = user.Id,
            User = user,
            Scope = CommunityPostScope.Neighborhood,
            NeighborhoodId = neighborhood.Id,
            Neighborhood = neighborhood
        };

        return new TestDomainGraph(
            province,
            district,
            neighborhood,
            user,
            department,
            category,
            organization,
            organizationDepartment,
            organizationDepartmentCategory,
            report,
            suggestion,
            announcement,
            communityPost);
    }

    private sealed record TestDomainGraph(
        Province Province,
        District District,
        Neighborhood Neighborhood,
        User User,
        Department Department,
        ReportCategory Category,
        Organization Organization,
        OrganizationDepartment OrganizationDepartment,
        OrganizationDepartmentCategory OrganizationDepartmentCategory,
        Report Report,
        Suggestion Suggestion,
        Announcement Announcement,
        CommunityPost CommunityPost);
}
