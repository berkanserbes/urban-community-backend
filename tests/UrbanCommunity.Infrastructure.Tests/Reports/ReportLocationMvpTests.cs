using MassTransit;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Features.Reports.Commands.CreateReport;
using UrbanCommunity.Application.Features.Reports.Commands.UpdateReport;
using UrbanCommunity.Application.Features.Reports.Queries.GetReportById;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Infrastructure.Persistence;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Reports;

public sealed class ReportLocationMvpTests
{
    [Fact]
    public async Task Create_WithExistingNeighborhoodWithoutPolygon_StoresIssueLocation()
    {
        await using var context = CreateContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            UserName = "test@example.com",
            Email = "test@example.com",
            IsActive = true
        };
        var neighborhood = new Neighborhood
        {
            Name = "Test Mahallesi",
            District = "Test Ilcesi",
            City = "Test Ili",
            DistrictId = Guid.NewGuid(),
            PolygonData = null
        };
        var category = new ReportCategory { Name = "Yol" };

        context.Users.Add(user);
        context.Neighborhoods.Add(neighborhood);
        context.ReportCategories.Add(category);
        await context.SaveChangesAsync();

        var bus = Bus.Factory.CreateUsingInMemory(_ => { });
        await bus.StartAsync();

        try
        {
            var handler = new CreateReportCommandHandler(context, bus, null!);
            var result = await handler.Handle(
                new CreateReportCommand(
                    "Yol sorunu",
                    "Yolda buyuk bir cukur bulunuyor.",
                    40.990123,
                    29.028456,
                    category.Id,
                    neighborhood.Id,
                    user.Id),
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data!.Latitude);
            Assert.NotNull(result.Data.Longitude);
            Assert.Equal(40.990123, result.Data.Latitude.Value, 6);
            Assert.Equal(29.028456, result.Data.Longitude.Value, 6);
            Assert.Equal(neighborhood.Id, result.Data.NeighborhoodId);

            var storedReport = await context.Reports.SingleAsync();
            Assert.NotNull(storedReport.Location);
            Assert.Equal(4326, storedReport.Location!.SRID);
            Assert.Equal(29.028456, storedReport.Location.X, 6);
            Assert.Equal(40.990123, storedReport.Location.Y, 6);
        }
        finally
        {
            await bus.StopAsync();
        }
    }

    [Fact]
    public async Task Create_WithoutCoordinates_StoresNeighborhoodOnly()
    {
        await using var context = CreateContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "test@example.com",
            Email = "test@example.com",
            IsActive = true
        };
        var neighborhood = new Neighborhood
        {
            Name = "Test Mahallesi",
            District = "Test Ilcesi",
            City = "Test Ili",
            DistrictId = Guid.NewGuid()
        };
        var category = new ReportCategory { Name = "Yol" };

        context.AddRange(user, neighborhood, category);
        await context.SaveChangesAsync();

        var bus = Bus.Factory.CreateUsingInMemory(_ => { });
        await bus.StartAsync();

        try
        {
            var handler = new CreateReportCommandHandler(context, bus, null!);
            var result = await handler.Handle(
                new CreateReportCommand(
                    "Yol sorunu",
                    "Adres bilgisi aciklama alaninda bulunuyor.",
                    null,
                    null,
                    category.Id,
                    neighborhood.Id,
                    user.Id),
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Data!.Latitude);
            Assert.Null(result.Data.Longitude);
            Assert.Equal(neighborhood.Id, result.Data.NeighborhoodId);
            Assert.Null((await context.Reports.SingleAsync()).Location);
        }
        finally
        {
            await bus.StopAsync();
        }
    }

    [Fact]
    public async Task Create_WithUnknownNeighborhood_ThrowsNotFound()
    {
        await using var context = CreateContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "test@example.com",
            Email = "test@example.com",
            IsActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new CreateReportCommandHandler(context, null!, null!);
        var command = new CreateReportCommand(
            "Yol sorunu",
            "Yolda buyuk bir cukur bulunuyor.",
            40.990123,
            29.028456,
            null,
            Guid.NewGuid(),
            user.Id);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Theory]
    [InlineData(null, 29.0)]
    [InlineData(41.0, null)]
    [InlineData(91.0, 29.0)]
    [InlineData(-91.0, 29.0)]
    [InlineData(41.0, 181.0)]
    [InlineData(41.0, -181.0)]
    [InlineData(double.NaN, 29.0)]
    [InlineData(41.0, double.PositiveInfinity)]
    public void CreateValidator_RejectsPartialOrInvalidCoordinates(double? latitude, double? longitude)
    {
        var validator = new CreateReportCommandValidator();
        var command = new CreateReportCommand(
            "Yol sorunu",
            "Yolda buyuk bir cukur bulunuyor.",
            latitude,
            longitude,
            null,
            Guid.NewGuid(),
            Guid.NewGuid());

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateValidator_AcceptsMissingCoordinates()
    {
        var validator = new CreateReportCommandValidator();
        var command = new CreateReportCommand(
            "Yol sorunu",
            "Adres bilgisi aciklama alaninda bulunuyor.",
            null,
            null,
            null,
            Guid.NewGuid(),
            Guid.NewGuid());

        Assert.True(validator.Validate(command).IsValid);
    }

    [Theory]
    [InlineData(null, 29.0)]
    [InlineData(41.0, null)]
    [InlineData(91.0, 29.0)]
    public void UpdateValidator_RejectsPartialOrInvalidCoordinates(double? latitude, double? longitude)
    {
        var validator = new UpdateReportCommandValidator();
        var command = new UpdateReportCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Yol sorunu",
            "Adres bilgisi aciklama alaninda bulunuyor.",
            latitude,
            longitude,
            null,
            Guid.NewGuid());

        Assert.False(validator.Validate(command).IsValid);
    }

    [Fact]
    public void UpdateValidator_AcceptsMissingCoordinates()
    {
        var validator = new UpdateReportCommandValidator();
        var command = new UpdateReportCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Yol sorunu",
            "Adres bilgisi aciklama alaninda bulunuyor.",
            null,
            null,
            null,
            Guid.NewGuid());

        Assert.True(validator.Validate(command).IsValid);
    }

    [Fact]
    public async Task Detail_ReturnsStoredCoordinatesAndNeighborhood()
    {
        await using var context = CreateContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            UserName = "test@example.com",
            Email = "test@example.com"
        };
        var neighborhood = new Neighborhood
        {
            Name = "Test Mahallesi",
            District = "Test Ilcesi",
            City = "Test Ili",
            DistrictId = Guid.NewGuid()
        };
        var category = new ReportCategory { Name = "Yol" };
        var report = new Report
        {
            Title = "Yol sorunu",
            Description = "Yolda buyuk bir cukur bulunuyor.",
            Location = new NetTopologySuite.Geometries.Point(29.028456, 40.990123) { SRID = 4326 },
            User = user,
            UserId = user.Id,
            Neighborhood = neighborhood,
            NeighborhoodId = neighborhood.Id,
            Category = category,
            CategoryId = category.Id
        };

        context.Reports.Add(report);
        await context.SaveChangesAsync();

        var handler = new GetReportByIdQueryHandler(context);
        var result = await handler.Handle(new GetReportByIdQuery(report.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data!.Latitude);
        Assert.NotNull(result.Data.Longitude);
        Assert.Equal(40.990123, result.Data.Latitude.Value, 6);
        Assert.Equal(29.028456, result.Data.Longitude.Value, 6);
        Assert.Equal(neighborhood.Id, result.Data.NeighborhoodId);
        Assert.Equal(neighborhood.Name, result.Data.NeighborhoodName);
    }

    [Fact]
    public async Task Detail_ReturnsNullCoordinatesForNeighborhoodOnlyReport()
    {
        await using var context = CreateContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            UserName = "test@example.com",
            Email = "test@example.com"
        };
        var neighborhood = new Neighborhood
        {
            Name = "Test Mahallesi",
            District = "Test Ilcesi",
            City = "Test Ili",
            DistrictId = Guid.NewGuid()
        };
        var category = new ReportCategory { Name = "Yol" };
        var report = new Report
        {
            Title = "Yol sorunu",
            Description = "Adres bilgisi aciklama alaninda bulunuyor.",
            Location = null,
            User = user,
            UserId = user.Id,
            Neighborhood = neighborhood,
            NeighborhoodId = neighborhood.Id,
            Category = category,
            CategoryId = category.Id
        };

        context.Reports.Add(report);
        await context.SaveChangesAsync();

        var result = await new GetReportByIdQueryHandler(context)
            .Handle(new GetReportByIdQuery(report.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Data!.Latitude);
        Assert.Null(result.Data.Longitude);
        Assert.Equal(neighborhood.Id, result.Data.NeighborhoodId);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
