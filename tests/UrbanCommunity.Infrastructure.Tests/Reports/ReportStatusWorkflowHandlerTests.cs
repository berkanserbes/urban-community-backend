using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Features.Reports.Commands.ChangeReportPriority;
using UrbanCommunity.Application.Features.Reports.Commands.ChangeReportStatus;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Persistence;
using UrbanCommunity.Infrastructure.Services;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Reports;

public sealed class ReportStatusWorkflowHandlerTests
{
    [Fact]
    public async Task ChangePriority_UpdatesPendingReviewReportPriority()
    {
        await using var context = CreateContext();
        var data = await SeedPendingReviewReportAsync(context);
        context.ChangeTracker.Clear();

        var handler = new ChangeReportPriorityCommandHandler(
            context,
            new ReportManagementAuthorizationService(context));

        var result = await handler.Handle(
            new ChangeReportPriorityCommand(
                data.ReportId,
                ReportPriority.High,
                data.AdminUserId,
                IsAdmin: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ReportPriority.High, result.Data.Priority);

        var report = await context.Reports
            .SingleAsync(candidate => candidate.Id == data.ReportId);

        Assert.Equal(ReportPriority.High, report.Priority);
        Assert.NotNull(report.UpdatedAt);
        Assert.Contains(
            context.AuditLogs,
            log => log.EntityType == nameof(Report) &&
                   log.EntityId == data.ReportId.ToString() &&
                   log.Action == EntityState.Modified.ToString());
    }

    [Fact]
    public async Task ChangeStatus_ConfirmsPendingReviewReportAndWritesHistory()
    {
        await using var context = CreateContext();
        var data = await SeedPendingReviewReportAsync(context);
        context.ChangeTracker.Clear();

        var publisher = new RecordingPublishEndpoint();
        var handler = new ChangeReportStatusCommandHandler(
            context,
            new ReportManagementAuthorizationService(context),
            publisher);

        var result = await handler.Handle(
            new ChangeReportStatusCommand(
                data.ReportId,
                ReportStatus.Confirmed,
                data.AdminUserId,
                IsAdmin: true,
                Note: " Onaylandi "),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ReportStatus.PendingReview, result.Data.FromStatus);
        Assert.Equal(ReportStatus.Confirmed, result.Data.ToStatus);

        var report = await context.Reports
            .Include(candidate => candidate.History)
            .SingleAsync(candidate => candidate.Id == data.ReportId);

        Assert.Equal(ReportStatus.Confirmed, report.Status);
        Assert.Null(report.ResolvedAt);

        var history = Assert.Single(report.History);
        Assert.Equal(ReportStatus.PendingReview, history.FromStatus);
        Assert.Equal(ReportStatus.Confirmed, history.ToStatus);
        Assert.Equal(data.AdminUserId, history.ChangedByUserId);
        Assert.Equal("Onaylandi", history.Note);

        var statusEvent = Assert.IsType<ReportStatusChangedEvent>(
            Assert.Single(publisher.PublishedMessages));
        Assert.Equal(data.ReportId, statusEvent.ReportId);
        Assert.Equal(ReportStatus.PendingReview, statusEvent.OldStatus);
        Assert.Equal(ReportStatus.Confirmed, statusEvent.NewStatus);
        Assert.Equal("Onaylandi", statusEvent.Note);
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

    private static async Task<TestData> SeedPendingReviewReportAsync(
        ApplicationDbContext context)
    {
        var province = new Province { Name = "Test Province" };
        var district = new District
        {
            Name = "Test District",
            Province = province
        };
        var neighborhood = new Neighborhood
        {
            Name = "Test Neighborhood",
            District = district.Name,
            City = province.Name,
            DistrictNavigation = district
        };
        var owner = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Report",
            LastName = "Owner",
            UserName = $"owner-{Guid.NewGuid():N}@example.com",
            Email = $"owner-{Guid.NewGuid():N}@example.com",
            Role = UserRole.Citizen,
            IsActive = true
        };
        var admin = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "System",
            LastName = "Admin",
            UserName = $"admin-{Guid.NewGuid():N}@example.com",
            Email = $"admin-{Guid.NewGuid():N}@example.com",
            Role = UserRole.Admin,
            IsActive = true
        };
        var category = new ReportCategory { Name = "Road" };
        var report = new Report
        {
            Title = "Broken pavement",
            Description = "Pavement needs repair",
            Status = ReportStatus.PendingReview,
            User = owner,
            Category = category,
            Neighborhood = neighborhood
        };

        context.AddRange(province, district, neighborhood, owner, admin, category, report);
        await context.SaveChangesAsync();

        context.AuditLogs.RemoveRange(context.AuditLogs);
        await context.SaveChangesAsync();

        return new TestData(report.Id, admin.Id);
    }

    private sealed record TestData(Guid ReportId, Guid AdminUserId);

    private sealed class RecordingPublishEndpoint : IPublishEndpoint
    {
        public List<object> PublishedMessages { get; } = [];

        public Task Publish<T>(
            T message,
            CancellationToken cancellationToken = default)
            where T : class
        {
            PublishedMessages.Add(message);
            return Task.CompletedTask;
        }

        public Task Publish<T>(
            T message,
            IPipe<PublishContext<T>> pipe,
            CancellationToken cancellationToken = default)
            where T : class =>
            Publish(message, cancellationToken);

        public Task Publish<T>(
            T message,
            IPipe<PublishContext> pipe,
            CancellationToken cancellationToken = default)
            where T : class =>
            Publish(message, cancellationToken);

        public Task Publish(
            object message,
            CancellationToken cancellationToken = default)
        {
            PublishedMessages.Add(message);
            return Task.CompletedTask;
        }

        public Task Publish(
            object message,
            Type messageType,
            CancellationToken cancellationToken = default) =>
            Publish(message, cancellationToken);

        public Task Publish(
            object message,
            IPipe<PublishContext> pipe,
            CancellationToken cancellationToken = default) =>
            Publish(message, cancellationToken);

        public Task Publish(
            object message,
            Type messageType,
            IPipe<PublishContext> pipe,
            CancellationToken cancellationToken = default) =>
            Publish(message, cancellationToken);

        Task IPublishEndpoint.Publish<T>(
            object message,
            CancellationToken cancellationToken) =>
            Publish(message, cancellationToken);

        Task IPublishEndpoint.Publish<T>(
            object message,
            IPipe<PublishContext<T>> pipe,
            CancellationToken cancellationToken) =>
            Publish(message, cancellationToken);

        Task IPublishEndpoint.Publish<T>(
            object message,
            IPipe<PublishContext> pipe,
            CancellationToken cancellationToken) =>
            Publish(message, cancellationToken);

        public ConnectHandle ConnectPublishObserver(IPublishObserver observer) =>
            new NoopConnectHandle();

        private sealed class NoopConnectHandle : ConnectHandle
        {
            public void Dispose()
            {
            }

            public void Disconnect()
            {
            }
        }
    }
}
