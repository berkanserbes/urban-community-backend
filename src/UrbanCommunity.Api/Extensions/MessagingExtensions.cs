using MassTransit;
using UrbanCommunity.Infrastructure.Consumers;
using UrbanCommunity.Infrastructure.Persistence;

namespace UrbanCommunity.Api.Extensions;

public static class MessagingExtensions
{
    /// <summary>
    /// Registers the consumers and the transactional outbox. The caller picks the transport:
    /// RabbitMQ in Program, the in-memory test harness in the integration tests.
    /// </summary>
    public static IBusRegistrationConfigurator AddMessaging(this IBusRegistrationConfigurator bus)
    {
        bus.SetKebabCaseEndpointNameFormatter();

        // Transactional outbox. Messages published while handling a request are saved in the
        // same transaction as the data and sent by a background delivery service after the
        // commit, so an unavailable broker can no longer lose them. On the consumer side the
        // inbox records every message id and skips redelivered messages.
        //
        // Keep the default RepeatableRead isolation. When two copies of a message race for the
        // inbox row, the loser gets a serialization failure and is retried. With ReadCommitted
        // it reads a stale inbox row instead and consumes the message a second time.
        bus.AddEntityFrameworkOutbox<ApplicationDbContext>(outbox =>
        {
            outbox.UsePostgres();
            outbox.UseBusOutbox();
        });

        // Retry has to wrap the outbox so that every attempt runs in a new transaction.
        // The AI review endpoint sets up its own retry and outbox in its consumer definition.
        bus.AddConfigureEndpointsCallback((context, name, endpoint) =>
        {
            if (name == ReportAiReviewRequestedConsumerDefinition.QueueName)
                return;

            endpoint.UseMessageRetry(retry => retry.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15)));
            endpoint.UseEntityFrameworkOutbox<ApplicationDbContext>(context);
        });

        bus.AddConsumer<
            ReportAiReviewRequestedConsumer,
            ReportAiReviewRequestedConsumerDefinition>();
        bus.AddConsumer<ReportAiReviewFaultConsumer>();
        bus.AddConsumer<ReportCreatedConsumer>();
        bus.AddConsumer<ReportStatusChangedConsumer>();
        bus.AddConsumer<ReportDepartmentChangedConsumer>();
        bus.AddConsumer<FileUploadConsumer>();
        bus.AddConsumer<AnnouncementPublishedConsumer>();
        bus.AddConsumer<SuggestionStatusChangedConsumer>();
        bus.AddConsumer<SuggestionOfficialResponseUpdatedConsumer>();

        return bus;
    }
}
