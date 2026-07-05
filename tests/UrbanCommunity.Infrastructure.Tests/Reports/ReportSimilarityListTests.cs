using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Features.Reports.Queries.GetReports;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Infrastructure.Persistence;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Reports;

public sealed class ReportSimilarityListTests
{
    [Fact]
    public async Task GetReports_ReturnsDistinctSimilarReportCountFromOutgoingAndIncomingMatches()
    {
        await using var context = CreateContext();
        var seed = await SeedReportsAsync(context);

        context.ReportSimilarityMatches.AddRange(
            new ReportSimilarityMatch
            {
                ReportId = seed.PrimaryReport.Id,
                Report = seed.PrimaryReport,
                SimilarReportId = seed.FirstSimilarReport.Id,
                SimilarReport = seed.FirstSimilarReport,
                SimilarityScore = 0.92,
                Reasoning = "Same issue"
            },
            new ReportSimilarityMatch
            {
                ReportId = seed.FirstSimilarReport.Id,
                Report = seed.FirstSimilarReport,
                SimilarReportId = seed.PrimaryReport.Id,
                SimilarReport = seed.PrimaryReport,
                SimilarityScore = 0.91,
                Reasoning = "Reverse match"
            },
            new ReportSimilarityMatch
            {
                ReportId = seed.SecondSimilarReport.Id,
                Report = seed.SecondSimilarReport,
                SimilarReportId = seed.PrimaryReport.Id,
                SimilarReport = seed.PrimaryReport,
                SimilarityScore = 0.86,
                Reasoning = "Incoming match"
            });
        await context.SaveChangesAsync();

        var handler = new GetReportsQueryHandler(context, null!);

        var result = await handler.Handle(new GetReportsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var primary = Assert.Single(result.Data!, report => report.Id == seed.PrimaryReport.Id);
        Assert.Equal(2, primary.SimilarReportCount);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<SeedData> SeedReportsAsync(ApplicationDbContext context)
    {
        var province = new Province { Name = "Province" };
        var district = new District
        {
            Name = "District",
            Province = province
        };
        var neighborhood = new Neighborhood
        {
            Name = "Neighborhood",
            District = district.Name,
            City = province.Name,
            DistrictNavigation = district
        };
        var category = new ReportCategory { Name = "Road" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Report",
            LastName = "Owner",
            UserName = $"{Guid.NewGuid():N}@example.com",
            Email = $"{Guid.NewGuid():N}@example.com",
            Neighborhood = neighborhood,
            IsActive = true
        };

        var primary = CreateReport("Primary", user, category, neighborhood);
        var firstSimilar = CreateReport("First similar", user, category, neighborhood);
        var secondSimilar = CreateReport("Second similar", user, category, neighborhood);

        context.AddRange(primary, firstSimilar, secondSimilar);
        await context.SaveChangesAsync();

        return new SeedData(primary, firstSimilar, secondSimilar);
    }

    private static Report CreateReport(
        string title,
        User user,
        ReportCategory category,
        Neighborhood neighborhood)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = $"{title} description",
            User = user,
            UserId = user.Id,
            Category = category,
            CategoryId = category.Id,
            Neighborhood = neighborhood,
            NeighborhoodId = neighborhood.Id
        };
    }

    private sealed record SeedData(
        Report PrimaryReport,
        Report FirstSimilarReport,
        Report SecondSimilarReport);
}
