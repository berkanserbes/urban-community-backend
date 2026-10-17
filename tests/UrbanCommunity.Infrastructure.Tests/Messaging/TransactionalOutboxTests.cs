using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using UrbanCommunity.Api.Extensions;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Features.Suggestions.Commands.ChangeSuggestionStatus;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Persistence;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Messaging;

/// <summary>
/// Runs the application's MassTransit setup (<see cref="MessagingExtensions.AddMessaging"/>)
/// against PostgreSQL in Docker, with the in-memory test transport. Requires a Docker engine.
/// </summary>
public sealed class TransactionalOutboxTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task EventPublishedByHandlerIsDeliveredAfterCommit()
    {
        await using var provider = BuildServices();
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        var (admin, suggestion) = await SeedSuggestionAsync();

        using (var scope = provider.CreateScope())
        {
            var handler = new ChangeSuggestionStatusCommandHandler(
                scope.ServiceProvider.GetRequiredService<IApplicationDbContext>(),
                scope.ServiceProvider.GetRequiredService<IPublishEndpoint>());
            var result = await handler.Handle(
                new ChangeSuggestionStatusCommand(suggestion.Id, admin.Id, SuggestionStatus.Reviewing),
                CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        Assert.True(await harness.Consumed.Any<SuggestionStatusChangedEvent>(
            message => message.Context.Message.SuggestionId == suggestion.Id));
        Assert.Equal(1, await CountNotificationsAsync(admin.Id, suggestion.Id, expected: 1));
    }

    [Fact]
    public async Task EventIsNotSentWhenTheTransactionIsNotSaved()
    {
        await using var provider = BuildServices();
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        var suggestionId = Guid.NewGuid();

        using (var scope = provider.CreateScope())
        {
            // Publish goes to the outbox of this unit of work; leaving the scope without
            // SaveChanges discards it, exactly like a request that fails after publishing.
            await scope.ServiceProvider.GetRequiredService<IPublishEndpoint>().Publish(
                new SuggestionStatusChangedEvent
                {
                    SuggestionId = suggestionId,
                    SuggestionTitle = "Not saved",
                    SuggestionOwnerId = Guid.NewGuid(),
                    OldStatus = SuggestionStatus.Active,
                    NewStatus = SuggestionStatus.Reviewing,
                    ChangedAt = DateTime.UtcNow
                });
        }

        Assert.False(await harness.Published.Any<SuggestionStatusChangedEvent>(
            message => message.Context.Message.SuggestionId == suggestionId));
    }

    [Fact]
    public async Task RedeliveredMessageIsConsumedOnlyOnce()
    {
        await using var provider = BuildServices();
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        var owner = await SeedUserAsync(UserRole.Citizen);
        var reportId = Guid.NewGuid();
        var duplicateId = NewId.NextGuid();

        // The same message delivered twice (as after a broker redelivery), then a different one.
        await harness.Bus.Publish(StatusChanged(reportId, owner.Id), context => context.MessageId = duplicateId);
        await harness.Bus.Publish(StatusChanged(reportId, owner.Id), context => context.MessageId = duplicateId);
        await harness.Bus.Publish(StatusChanged(reportId, owner.Id), context => context.MessageId = NewId.NextGuid());

        Assert.Equal(2, await CountNotificationsAsync(owner.Id, reportId, expected: 2));
    }

    private ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => postgres.Configure(options));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<IRealtimeNotificationService, NoOpRealtimeNotificationService>();
        services.AddMassTransitTestHarness(bus => bus.AddMessaging());
        return services.BuildServiceProvider(validateScopes: true);
    }

    private static ReportStatusChangedEvent StatusChanged(Guid reportId, Guid ownerId) => new()
    {
        ReportId = reportId,
        ReportTitle = "Inbox test",
        ReportOwnerId = ownerId,
        OldStatus = ReportStatus.PendingReview,
        NewStatus = ReportStatus.Confirmed,
        ChangedAt = DateTime.UtcNow
    };

    /// <summary>Waits until <paramref name="expected"/> notifications exist, then a bit longer to catch extras.</summary>
    private async Task<int> CountNotificationsAsync(Guid userId, Guid relatedEntityId, int expected)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        int count;
        do
        {
            await using var context = postgres.CreateContext();
            count = await context.Notifications.CountAsync(
                notification => notification.UserId == userId && notification.RelatedEntityId == relatedEntityId);
            if (count >= expected)
                break;
            await Task.Delay(200);
        } while (DateTime.UtcNow < deadline);

        await Task.Delay(TimeSpan.FromSeconds(2));
        await using var final = postgres.CreateContext();
        return await final.Notifications.CountAsync(
            notification => notification.UserId == userId && notification.RelatedEntityId == relatedEntityId);
    }

    private async Task<(User Admin, Suggestion Suggestion)> SeedSuggestionAsync()
    {
        var admin = await SeedUserAsync(UserRole.Admin);
        var suggestion = new Suggestion { Title = "Outbox test", Content = "Content", UserId = admin.Id };
        await using var context = postgres.CreateContext();
        context.Suggestions.Add(suggestion);
        await context.SaveChangesAsync();
        return (admin, suggestion);
    }

    private async Task<User> SeedUserAsync(UserRole role)
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = email,
            UserName = email,
            Role = role,
            IsActive = true
        };
        await using var context = postgres.CreateContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private sealed class NoOpRealtimeNotificationService : IRealtimeNotificationService
    {
        public Task SendToGroupAsync(string group, string eventName, object data, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SendToUserAsync(string userId, string eventName, object data, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}

/// <summary>PostgreSQL with PostGIS in Docker, migrated once per test class.</summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgis/postgis:17-3.5").Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public void Configure(DbContextOptionsBuilder options) =>
        options.UseNpgsql(_container.GetConnectionString(), npgsql => npgsql.UseNetTopologySuite());

    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>();
        Configure(options);
        return new ApplicationDbContext(options.Options);
    }
}
