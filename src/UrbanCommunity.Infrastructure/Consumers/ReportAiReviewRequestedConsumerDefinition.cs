using MassTransit;
using UrbanCommunity.Infrastructure.Persistence;

namespace UrbanCommunity.Infrastructure.Consumers;

public sealed class ReportAiReviewRequestedConsumerDefinition
    : ConsumerDefinition<ReportAiReviewRequestedConsumer>
{
    /// <summary>Queue name; the general endpoint callback in Program skips this endpoint.</summary>
    public const string QueueName = "report-ai-review-requested";

    public ReportAiReviewRequestedConsumerDefinition()
    {
        EndpointName = QueueName;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ReportAiReviewRequestedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        // Only transient AI failures are retried; anything else faults straight away and the
        // fault consumer completes the review. Retry wraps the outbox so that every attempt
        // runs in a new transaction.
        endpointConfigurator.UseMessageRetry(retry =>
        {
            retry.Handle<Services.Ai.AiServiceTransientException>();
            retry.Intervals(
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10));
        });
        endpointConfigurator.UseEntityFrameworkOutbox<ApplicationDbContext>(context);
    }
}
