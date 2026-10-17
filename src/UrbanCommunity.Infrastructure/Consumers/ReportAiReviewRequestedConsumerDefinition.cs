using MassTransit;
using Npgsql;
using UrbanCommunity.Infrastructure.Persistence;

namespace UrbanCommunity.Infrastructure.Consumers;

public sealed class ReportAiReviewRequestedConsumerDefinition
    : ConsumerDefinition<ReportAiReviewRequestedConsumer>
{
    /// <summary>Queue name; the general endpoint callback in AddMessaging skips this endpoint.</summary>
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
        // Only transient AI failures and database conflicts are retried; anything else faults
        // straight away and the fault consumer completes the review. Retry wraps the outbox so
        // that every attempt runs in a new transaction.
        endpointConfigurator.UseMessageRetry(retry =>
        {
            retry.Handle<Services.Ai.AiServiceTransientException>();
            retry.Handle<Exception>(IsDatabaseConflict);
            retry.Intervals(
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10));
        });
        endpointConfigurator.UseEntityFrameworkOutbox<ApplicationDbContext>(context);
    }

    /// <summary>
    /// A concurrent transaction changed the same rows, for example another copy of this message
    /// that holds the inbox row. A new attempt reads the committed data. EF Core can wrap the
    /// PostgreSQL error (InvalidOperationException, DbUpdateException), so the chain is searched.
    /// </summary>
    internal static bool IsDatabaseConflict(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgres)
                return postgres.SqlState is PostgresErrorCodes.SerializationFailure
                    or PostgresErrorCodes.DeadlockDetected
                    or PostgresErrorCodes.UniqueViolation;
        }

        return false;
    }
}
